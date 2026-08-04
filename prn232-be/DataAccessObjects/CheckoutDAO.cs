using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Common;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

/// <summary>
/// Tạo MỘT MasterInvoice cho mua lẻ hoặc mua GỘP nhiều module (xe + phụ tùng + dịch vụ) của cùng khách hàng.
/// Tái sử dụng đơn phụ tùng &amp; lịch dịch vụ ĐÃ tạo (không viết lại logic tạo đơn/lịch), chỉ gắn chúng
/// vào master chung, tạo hóa đơn con tương ứng, giữ chỗ xe &amp; trừ kho phụ tùng, rồi sinh mã captcha.
/// Việc thanh toán (đặt cọc/mua đứt) do MasterInvoicePaymentDAO xử lý thống nhất.
/// Singleton pattern theo backend_conventions.md.
/// </summary>
public class CheckoutDAO
{
    private static CheckoutDAO? _instance;
    private static readonly object _lock = new();
    private CheckoutDAO() { }

    public static CheckoutDAO Instance
    {
        get
        {
            lock (_lock)
            {
                _instance ??= new CheckoutDAO();
                return _instance;
            }
        }
    }

    /// <param name="actingUserId">Nhân viên lập (staff) hoặc chính khách hàng (self-service).</param>
    /// <param name="selfService">
    /// true = khách tự checkout: KHÔNG giữ chỗ xe, KHÔNG sinh captcha (nhân viên sẽ cấp mã sau),
    /// cọc chưa tính hạn (2 tuần tính từ khi khách xác nhận cọc).
    /// </param>
    public ServiceResult CreateInvoice(CheckoutDto dto, int actingUserId, bool selfService = false)
    {
        // 0. Tự động dọn dẹp các hóa đơn đã hết hạn (quá 30 phút chưa captcha hoặc quá 14 ngày cọc)
        try { MasterInvoicePaymentDAO.Instance.ReleaseExpiredInvoices(); } catch { /* ignore */ }

        if ((dto.Cars == null || dto.Cars.Count == 0) &&
            (dto.PartOrderIds == null || dto.PartOrderIds.Count == 0) &&
            (dto.AppointmentIds == null || dto.AppointmentIds.Count == 0))
            return ServiceResult.Fail("Hóa đơn phải có ít nhất một mục (xe, phụ tùng hoặc dịch vụ).");

        using var ctx = new CarShowroomContext();
        using var tx = ctx.Database.BeginTransaction();
        try
        {
            var customer = ctx.AppUsers.SingleOrDefault(u => u.UserId == dto.CustomerId);
            if (customer == null) return ServiceResult.Fail("Không tìm thấy khách hàng.");

            var now = DateTime.Now;
            bool isDeposit = dto.PurchaseType == "Deposit";
            decimal totalSubTotal = 0;

            var master = new MasterInvoice
            {
                InvoiceNumber = InvoiceNumberHelper.Next(ctx),
                CustomerId = dto.CustomerId,
                StaffId = selfService ? (int?)null : actingUserId,
                DiscountAmount = dto.DiscountAmount,
                TaxAmount = dto.TaxAmount,
                PurchaseType = dto.PurchaseType,
                PaymentStatus = PaymentStatuses.Unpaid,
                InvoiceStatus = InvoiceStatuses.PendingVerification,
                Notes = dto.Notes,
                CreatedAt = now,
                CreatedUser = actingUserId,
                ExpiredAt = now.AddMinutes(30), // Khóa tạm thời 30 phút để hoàn tất xác thực captcha lớp 1
                TotalSubTotal = 0,
                TotalAmount = 0
            };
            ctx.MasterInvoices.Add(master);
            ctx.SaveChanges(); // sinh MasterInvoiceId

            bool hasCar = false, hasPart = false, hasService = false;

            // ---------- XE ----------
            foreach (var line in dto.Cars ?? new List<CheckoutCarLineDto>())
            {
                var request = ctx.PurchaseRequests.SingleOrDefault(r => r.RequestId == line.PurchaseRequestId);
                if (request == null) return Rollback(tx, $"Không tìm thấy yêu cầu mua xe #{line.PurchaseRequestId}.");
                if (request.CustomerId != dto.CustomerId) return Rollback(tx, $"Yêu cầu mua #{line.PurchaseRequestId} không thuộc khách hàng này.");
                if (request.Status is "Rejected" or "Completed") return Rollback(tx, $"Yêu cầu mua #{line.PurchaseRequestId} đã kết thúc.");

                var car = ctx.Cars.SingleOrDefault(c => c.CarId == request.CarId);
                if (car == null) return Rollback(tx, "Không tìm thấy xe.");
                if (car.Status == "Inactive") return Rollback(tx, $"Xe '{car.CarName}' hiện ngưng kinh doanh.");

                // Lấy tất cả các hóa đơn active khác đang chứa xe này
                var existingCarInvoices = ctx.CarInvoices
                    .Include(c => c.MasterInvoice)
                    .Where(c => c.CarId == car.CarId &&
                                c.MasterInvoice.InvoiceStatus != InvoiceStatuses.Cancelled &&
                                c.MasterInvoiceId != master.MasterInvoiceId)
                    .ToList();

                // Rule 1: Nếu có hóa đơn cũ ĐÃ ĐƯỢC CONFIRM CAPTCHA 1 lần (đã cọc hoặc đã thanh toán) -> Báo lỗi không thể tạo mới
                var confirmedInvoice = existingCarInvoices.FirstOrDefault(c =>
                    c.MasterInvoice.IsDepositCaptchaUsed ||
                    c.MasterInvoice.IsFinalCaptchaUsed ||
                    c.MasterInvoice.PaymentStatus == PaymentStatuses.Deposited ||
                    c.MasterInvoice.PaymentStatus == PaymentStatuses.Paid);

                if (confirmedInvoice != null || car.Status == "Sold")
                {
                    string invNo = confirmedInvoice?.MasterInvoice.InvoiceNumber ?? "trước đó";
                    return Rollback(tx, $"Xe '{car.CarName}' đã được xác thực đặt cọc hoặc mua đứt ở hóa đơn #{invNo} và không thể tạo thêm hóa đơn.");
                }

                // Rule 2: Các hóa đơn cũ CHƯA XÁC THỰC LỚP NÀO -> Cho phép đè lên, nhận hóa đơn mới nhất và disable hóa đơn cũ
                var unconfirmedCarInvoices = existingCarInvoices.Where(c =>
                    !c.MasterInvoice.IsDepositCaptchaUsed &&
                    !c.MasterInvoice.IsFinalCaptchaUsed &&
                    c.MasterInvoice.PaymentStatus == PaymentStatuses.Unpaid).ToList();

                foreach (var oldCi in unconfirmedCarInvoices)
                {
                    var oldMaster = oldCi.MasterInvoice;
                    oldMaster.InvoiceStatus = InvoiceStatuses.Cancelled;
                    oldMaster.Notes = (string.IsNullOrEmpty(oldMaster.Notes) ? "" : oldMaster.Notes + " | ")
                        + $"Vô hiệu hóa do bị thay thế bởi hóa đơn mới #{master.InvoiceNumber} ({now:dd/MM/yyyy HH:mm}).";
                    oldMaster.UpdatedAt = now;

                    // Nếu hóa đơn cũ bị hủy đã lỡ trừ tồn kho phụ tùng -> Hoàn lại tồn kho
                    foreach (var pi in ctx.PartInvoices.Where(p => p.MasterInvoiceId == oldMaster.MasterInvoiceId).ToList())
                    {
                        var order = ctx.PartOrders.Include(o => o.PartOrderDetails).SingleOrDefault(o => o.OrderId == pi.PartOrderId);
                        if (order != null)
                        {
                            foreach (var detail in order.PartOrderDetails)
                            {
                                var partItem = ctx.Parts.SingleOrDefault(p => p.PartId == detail.PartId);
                                if (partItem != null)
                                {
                                    partItem.Quantity += detail.Quantity;
                                    if (partItem.Status == "OutOfStock" && partItem.Quantity > 0) partItem.Status = "Available";
                                }
                            }
                        }
                    }
                }

                decimal carSub = car.Price + line.RegistrationFee + line.PlateFee + line.InsuranceFee;
                totalSubTotal += carSub;

                ctx.CarInvoices.Add(new CarInvoice
                {
                    MasterInvoiceId = master.MasterInvoiceId,
                    CarId = car.CarId,
                    PurchaseRequestId = request.RequestId,
                    UnitPrice = car.Price,
                    RegistrationFee = line.RegistrationFee,
                    PlateFee = line.PlateFee,
                    InsuranceFee = line.InsuranceFee,
                    Notes = dto.Notes,
                    CreatedAt = now,
                    CreatedUser = actingUserId
                });

                // Khóa số lượng xe tạm thời (Reserved) 30 phút
                car.Status = "Reserved";
                request.Status = "Confirmed";
                request.UpdatedAt = now;
                request.UpdatedUser = actingUserId;
                hasCar = true;
            }

            // ---------- PHỤ TÙNG ----------
            foreach (var orderId in (dto.PartOrderIds ?? new List<int>()).Distinct())
            {
                var order = ctx.PartOrders.Include(o => o.PartOrderDetails).SingleOrDefault(o => o.OrderId == orderId);
                if (order == null) return Rollback(tx, $"Không tìm thấy đơn phụ tùng #{orderId}.");
                if (order.CustomerId != dto.CustomerId) return Rollback(tx, $"Đơn phụ tùng #{orderId} không thuộc khách hàng này.");
                if (order.MasterInvoiceId.HasValue || ctx.PartInvoices.Any(p => p.PartOrderId == orderId))
                    return Rollback(tx, $"Đơn phụ tùng #{orderId} đã được lập hóa đơn.");
                if (order.Status is "Cancelled" or "Completed") return Rollback(tx, $"Đơn phụ tùng #{orderId} không hợp lệ để thanh toán.");
                if (!order.PartOrderDetails.Any()) return Rollback(tx, $"Đơn phụ tùng #{orderId} không có sản phẩm.");

                decimal detailsSubtotal = 0;
                foreach (var detail in order.PartOrderDetails)
                {
                    var part = ctx.Parts.SingleOrDefault(p => p.PartId == detail.PartId);
                    if (part == null) return Rollback(tx, $"Không tìm thấy phụ tùng ID #{detail.PartId}.");
                    if (part.Quantity < detail.Quantity)
                        return Rollback(tx, $"Tồn kho '{part.PartName}' không đủ (còn {part.Quantity}).");

                    part.Quantity -= detail.Quantity;
                    if (part.Quantity == 0 || part.Quantity < part.MinStockLevel) part.Status = "OutOfStock";

                    ctx.InventoryTransactions.Add(new InventoryTransaction
                    {
                        PartId = part.PartId,
                        TransactionType = InventoryTransactionTypes.Export,
                        Quantity = -detail.Quantity,
                        ReferenceType = InventoryReferenceTypes.PartOrder,
                        ReferenceId = order.OrderId,
                        StaffId = actingUserId,
                        Notes = $"Xuất kho theo hóa đơn tổng #{master.MasterInvoiceId} (đơn phụ tùng #{order.OrderId})",
                        TransactionDate = now,
                        CreatedAt = now,
                        CreatedUser = actingUserId
                    });

                    detailsSubtotal += detail.UnitPrice * detail.Quantity;
                }

                totalSubTotal += detailsSubtotal + order.ShippingFee;

                ctx.PartInvoices.Add(new PartInvoice
                {
                    MasterInvoiceId = master.MasterInvoiceId,
                    PartOrderId = order.OrderId,
                    SubTotal = detailsSubtotal,
                    ShippingFee = order.ShippingFee,
                    TaxAmount = 0,
                    CreatedAt = now,
                    CreatedUser = actingUserId
                });

                order.MasterInvoiceId = master.MasterInvoiceId;
                if (order.Status == "Pending") order.Status = "Confirmed";
                order.UpdatedAt = now;
                hasPart = true;
            }

            // ---------- DỊCH VỤ ----------
            foreach (var apptId in (dto.AppointmentIds ?? new List<int>()).Distinct())
            {
                var appt = ctx.MaintenanceAppointments
                    .Include(a => a.AppointmentDetails)
                    .Include(a => a.ConsumedParts)
                    .SingleOrDefault(a => a.AppointmentId == apptId);
                if (appt == null) return Rollback(tx, $"Không tìm thấy lịch dịch vụ #{apptId}.");
                if (appt.CustomerId != dto.CustomerId) return Rollback(tx, $"Lịch dịch vụ #{apptId} không thuộc khách hàng này.");
                if (appt.MasterInvoiceId.HasValue || ctx.ServiceInvoices.Any(s => s.AppointmentId == apptId))
                    return Rollback(tx, $"Lịch dịch vụ #{apptId} đã được lập hóa đơn.");
                if (appt.Status == "Cancelled") return Rollback(tx, $"Lịch dịch vụ #{apptId} đã bị hủy.");

                decimal detailsTotal = appt.AppointmentDetails?.Sum(d => d.UnitPrice * d.Quantity) ?? 0;
                // Phụ tùng tiêu chuẩn (khách chọn khi đặt, IsIncurred=false) luôn tính; phụ tùng phát sinh chỉ tính khi đã duyệt.
                decimal partsTotal = appt.ConsumedParts?.Where(p => !p.IsIncurred || p.ApprovedByCustomer).Sum(p => p.UnitPrice * p.Quantity) ?? 0;
                decimal svcSub = detailsTotal + partsTotal;
                totalSubTotal += svcSub;

                ctx.ServiceInvoices.Add(new ServiceInvoice
                {
                    MasterInvoiceId = master.MasterInvoiceId,
                    AppointmentId = appt.AppointmentId,
                    SubTotal = svcSub,
                    LaborDiscount = 0,
                    CreatedAt = now,
                    CreatedUser = actingUserId
                });

                appt.MasterInvoiceId = master.MasterInvoiceId;
                appt.UpdatedAt = now;
                hasService = true;
            }

            // ---------- Tổng hợp & phân loại ----------
            int moduleCount = (hasCar ? 1 : 0) + (hasPart ? 1 : 0) + (hasService ? 1 : 0);
            master.InvoiceType = moduleCount > 1
                ? InvoiceTypesExtra.Combined
                : hasCar ? InvoiceTypes.Car : hasPart ? InvoiceTypes.Part : InvoiceTypes.Service;

            master.TotalSubTotal = totalSubTotal;
            master.TotalAmount = totalSubTotal - dto.DiscountAmount + dto.TaxAmount;
            if (master.TotalAmount < 0) return Rollback(tx, "Chiết khấu vượt quá tổng tiền.");

            // ---------- Cọc & captcha ----------
            if (isDeposit)
            {
                decimal deposit = dto.DepositAmount ?? Math.Round(master.TotalAmount * 0.10m, 0);
                if (deposit <= 0 || deposit > master.TotalAmount) return Rollback(tx, "Số tiền cọc không hợp lệ.");
                master.DepositAmount = deposit;
                master.IsDepositCaptchaUsed = false;
                if (!selfService)
                {
                    // Nhân viên tạo hóa đơn: sinh mã ngay, hạn cọc tính từ bây giờ.
                    master.DepositExpiresAt = now.AddDays(dto.DepositExpiresInDays);
                    master.ExpiredAt = master.DepositExpiresAt;
                    master.DepositCaptchaCode = CaptchaHelper.Generate();
                }
                // selfService: chưa sinh mã (nhân viên cấp sau); hạn 2 tuần tính từ khi khách xác nhận cọc.
            }
            else
            {
                master.IsFinalCaptchaUsed = false;
                if (!selfService)
                    master.FinalCaptchaCode = CaptchaHelper.Generate();
            }

            ctx.SaveChanges();
            tx.Commit();

            var captcha = selfService ? null : (isDeposit ? master.DepositCaptchaCode : master.FinalCaptchaCode);
            return ServiceResult.Ok(
                selfService
                    ? "Đã tạo hóa đơn. Vui lòng vào trang Hóa đơn của tôi, liên hệ nhân viên để nhận mã và xác thực."
                    : "Đã tạo hóa đơn tổng và sinh mã xác thực. Cung cấp mã cho khách hàng.",
                new
                {
                    masterInvoiceId = master.MasterInvoiceId,
                    invoiceNumber = master.InvoiceNumber,
                    invoiceType = master.InvoiceType,
                    purchaseType = master.PurchaseType,
                    totalAmount = master.TotalAmount,
                    depositAmount = master.DepositAmount,
                    depositExpiresAt = master.DepositExpiresAt,
                    captchaCode = captcha
                });
        }
        catch (Exception ex)
        {
            try { tx.Rollback(); } catch { /* ignore */ }
            return ServiceResult.Fail("Lỗi hệ thống khi tạo hóa đơn: " + (ex.InnerException?.Message ?? ex.Message));
        }
    }

    private static ServiceResult Rollback(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx, string message)
    {
        tx.Rollback();
        return ServiceResult.Fail(message);
    }
}
