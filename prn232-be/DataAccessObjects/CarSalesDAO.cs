using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Common;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

/// <summary>
/// Yêu cầu mua xe của khách hàng (module ô tô).
/// Việc phát hành hóa đơn + đặt cọc/mua đứt + xác thực captcha được xử lý ở tầng dùng chung
/// (CheckoutDAO tạo hóa đơn tổng, MasterInvoicePaymentDAO xử lý thanh toán) để thống nhất cho cả 3 module.
/// Singleton pattern theo backend_conventions.md.
/// </summary>
public class CarSalesDAO
{
    private static CarSalesDAO? _instance;
    private static readonly object _lock = new();
    private CarSalesDAO() { }

    public static CarSalesDAO Instance
    {
        get
        {
            lock (_lock)
            {
                _instance ??= new CarSalesDAO();
                return _instance;
            }
        }
    }

    public ServiceResult CreatePurchaseRequest(CreatePurchaseRequestDto dto, int customerId)
    {
        using var ctx = new CarShowroomContext();

        // 0. Dọn các hóa đơn hết hạn (quá 30 phút chưa captcha hoặc quá 14 ngày cọc)
        try { MasterInvoicePaymentDAO.Instance.ReleaseExpiredInvoices(); } catch { /* ignore */ }

        var car = ctx.Cars.SingleOrDefault(c => c.CarId == dto.CarId);
        if (car == null) return ServiceResult.Fail("Không tìm thấy xe.");
        if (car.Status is "Sold" or "Inactive") return ServiceResult.Fail("Xe này hiện không còn được bán.");

        // Kiểm tra xem xe này đã có hóa đơn nào ĐÃ CONFIRM CAPTCHA (đã cọc hoặc đã mua đứt) hay chưa
        var confirmedCarInvoice = ctx.CarInvoices
            .Include(c => c.MasterInvoice)
            .FirstOrDefault(c => c.CarId == dto.CarId &&
                                 c.MasterInvoice.InvoiceStatus != InvoiceStatuses.Cancelled &&
                                 (c.MasterInvoice.IsDepositCaptchaUsed ||
                                  c.MasterInvoice.IsFinalCaptchaUsed ||
                                  c.MasterInvoice.PaymentStatus == PaymentStatuses.Deposited ||
                                  c.MasterInvoice.PaymentStatus == PaymentStatuses.Paid));

        if (confirmedCarInvoice != null || car.Status == "Sold")
        {
            return ServiceResult.Fail($"Xe '{car.CarName}' đã được xác thực đặt cọc hoặc mua đứt ở hóa đơn #{confirmedCarInvoice?.MasterInvoice.InvoiceNumber}. Vui lòng vào trang 'Hóa đơn của tôi'.");
        }

        // Nếu xe chưa được xác thực captcha lớp nào -> cho phép dùng lại yêu cầu Pending hoặc tạo yêu cầu mới
        var existingPending = ctx.PurchaseRequests
            .Where(p => p.CarId == dto.CarId && p.CustomerId == customerId && p.Status == "Pending")
            .OrderByDescending(p => p.RequestId)
            .FirstOrDefault();
        if (existingPending != null)
        {
            return ServiceResult.Ok("Dùng lại yêu cầu mua đang chờ cho xe này.", new { requestId = existingPending.RequestId });
        }

        var now = DateTime.Now;
        var request = new PurchaseRequest
        {
            CarId = dto.CarId,
            CustomerId = customerId,
            CustomerName = dto.CustomerName.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            CustomerEmail = dto.CustomerEmail?.Trim(),
            Message = dto.Message?.Trim(),
            Status = "Pending",
            CreatedAt = now,
            CreatedUser = customerId
        };
        ctx.PurchaseRequests.Add(request);
        ctx.SaveChanges();

        return ServiceResult.Ok(
            "Đã gửi yêu cầu mua xe. Vui lòng liên hệ nhân viên để được lập hóa đơn và nhận mã xác thực.",
            new { requestId = request.RequestId });
    }

    public IEnumerable<PurchaseRequest> GetPurchaseRequests(int? customerId)
    {
        using var ctx = new CarShowroomContext();
        IQueryable<PurchaseRequest> query = ctx.PurchaseRequests.AsNoTracking().Include(p => p.Car);
        if (customerId.HasValue) query = query.Where(p => p.CustomerId == customerId.Value);
        return query.OrderByDescending(p => p.CreatedAt).ToList();
    }
}
