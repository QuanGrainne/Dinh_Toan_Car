using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Common;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

/// <summary>
/// Tầng THANH TOÁN DÙNG CHUNG trên MasterInvoice cho cả 3 module (xe / phụ tùng / dịch vụ) và hóa đơn gộp.
/// Nghiệp vụ đặt cọc (Deposit) &amp; mua đứt (Buyout) đều xác thực bằng mã captcha do NHÂN VIÊN sinh,
/// khách hàng nhập để xác thực hóa đơn. Sau khi xác thực, side-effect theo từng loại hóa đơn con
/// được áp dụng thống nhất qua dispatcher (xe: giữ chỗ/bán; phụ tùng: đơn đã thanh toán; dịch vụ: đánh dấu đã trả).
/// Singleton pattern theo backend_conventions.md.
/// </summary>
public class MasterInvoicePaymentDAO
{
    private static MasterInvoicePaymentDAO? _instance;
    private static readonly object _lock = new();
    private MasterInvoicePaymentDAO() { }

    public static MasterInvoicePaymentDAO Instance
    {
        get
        {
            lock (_lock)
            {
                _instance ??= new MasterInvoicePaymentDAO();
                return _instance;
            }
        }
    }

    // ---------------------------------------------------------------------
    // NHÂN VIÊN: sinh mã captcha
    // ---------------------------------------------------------------------

    public ServiceResult GenerateDepositCaptcha(int masterInvoiceId, decimal? depositAmount, int expiresInDays, int staffId)
    {
        using var ctx = new CarShowroomContext();
        var master = ctx.MasterInvoices.SingleOrDefault(m => m.MasterInvoiceId == masterInvoiceId);
        if (master == null) return ServiceResult.Fail("Không tìm thấy hóa đơn.");
        if (master.IsFinalCaptchaUsed || master.PaymentStatus == PaymentStatuses.Paid)
            return ServiceResult.Fail("Hóa đơn đã thanh toán, không thể tạo mã đặt cọc.");
        if (master.InvoiceStatus == InvoiceStatuses.Cancelled)
            return ServiceResult.Fail("Hóa đơn đã bị hủy.");

        var now = DateTime.Now;
        // Ưu tiên số cọc truyền vào; nếu không có thì giữ số cọc đã lập ở checkout; cuối cùng mặc định 10%.
        decimal deposit = depositAmount ?? master.DepositAmount ?? Math.Round(master.TotalAmount * 0.10m, 0);
        if (deposit <= 0 || deposit > master.TotalAmount)
            return ServiceResult.Fail("Số tiền cọc không hợp lệ.");

        master.PurchaseType = "Deposit";
        master.DepositAmount = deposit;
        // KHÔNG đặt hạn cọc ở đây — hạn giữ chỗ 2 tuần chỉ bắt đầu khi khách xác nhận cọc thành công.
        master.DepositCaptchaCode = CaptchaHelper.Generate();
        master.IsDepositCaptchaUsed = false;
        master.DepositCaptchaUsedAt = null;
        if (master.InvoiceStatus == InvoiceStatuses.Pending)
            master.InvoiceStatus = InvoiceStatuses.PendingVerification;
        master.UpdatedAt = now;
        master.UpdatedUser = staffId;
        ctx.SaveChanges();

        return ServiceResult.Ok("Đã sinh mã đặt cọc. Cung cấp mã cho khách để xác thực.",
            new { masterInvoiceId, captchaCode = master.DepositCaptchaCode, depositAmount = deposit });
    }

    public ServiceResult GenerateFinalCaptcha(int masterInvoiceId, int staffId)
    {
        using var ctx = new CarShowroomContext();
        var master = ctx.MasterInvoices.SingleOrDefault(m => m.MasterInvoiceId == masterInvoiceId);
        if (master == null) return ServiceResult.Fail("Không tìm thấy hóa đơn.");
        if (master.IsFinalCaptchaUsed || master.PaymentStatus == PaymentStatuses.Paid)
            return ServiceResult.Fail("Hóa đơn đã thanh toán trước đó.");
        if (master.InvoiceStatus == InvoiceStatuses.Cancelled)
            return ServiceResult.Fail("Hóa đơn đã bị hủy.");
        if (master.PurchaseType == "Deposit" && master.PaymentStatus != PaymentStatuses.Deposited)
            return ServiceResult.Fail("Hóa đơn đặt cọc cần khách xác thực cọc trước khi tất toán.");

        var now = DateTime.Now;
        master.FinalCaptchaCode = CaptchaHelper.Generate();
        master.IsFinalCaptchaUsed = false;
        master.FinalCaptchaUsedAt = null;
        if (master.InvoiceStatus == InvoiceStatuses.Pending)
            master.InvoiceStatus = InvoiceStatuses.PendingVerification;
        master.UpdatedAt = now;
        master.UpdatedUser = staffId;
        ctx.SaveChanges();

        return ServiceResult.Ok("Đã sinh mã tất toán/mua đứt. Cung cấp mã cho khách để xác thực.",
            new { masterInvoiceId, captchaCode = master.FinalCaptchaCode });
    }

    // ---------------------------------------------------------------------
    // KHÁCH HÀNG: xác thực captcha
    // ---------------------------------------------------------------------

    public ServiceResult VerifyDeposit(int masterInvoiceId, string code, int customerId)
    {
        using var ctx = new CarShowroomContext();
        using var tx = ctx.Database.BeginTransaction();
        try
        {
            var master = ctx.MasterInvoices.SingleOrDefault(m => m.MasterInvoiceId == masterInvoiceId);
            if (master == null) return ServiceResult.Fail("Không tìm thấy hóa đơn.");
            if (master.CustomerId != customerId) return ServiceResult.Fail("Bạn không có quyền xác thực hóa đơn này.");
            if (master.PurchaseType != "Deposit") return ServiceResult.Fail("Hóa đơn này không phải hóa đơn đặt cọc.");
            if (master.IsDepositCaptchaUsed) return ServiceResult.Fail("Mã đặt cọc đã được sử dụng.");
            if (master.DepositExpiresAt.HasValue && master.DepositExpiresAt.Value < DateTime.Now)
                return ServiceResult.Fail("Mã đặt cọc đã hết hạn. Vui lòng liên hệ nhân viên để được cấp lại.");
            if (!CaptchaHelper.Match(master.DepositCaptchaCode, code))
                return ServiceResult.Fail("Mã xác thực không đúng.");

            var now = DateTime.Now;
            master.IsDepositCaptchaUsed = true;
            master.DepositCaptchaUsedAt = now;
            master.DepositPaidAmount = master.DepositAmount;
            master.PaymentStatus = PaymentStatuses.Deposited;
            master.InvoiceStatus = InvoiceStatuses.Confirmed;
            // Cọc giữ chỗ tối đa 2 tuần tính từ lúc khách xác nhận cọc.
            master.DepositExpiresAt = now.AddDays(14);
            master.ExpiredAt = master.DepositExpiresAt;
            master.UpdatedAt = now;
            master.UpdatedUser = customerId;

            ApplyDepositEffects(ctx, master, now);

            ctx.SaveChanges();
            tx.Commit();
            return ServiceResult.Ok("Xác thực đặt cọc thành công!",
                new { masterInvoiceId, paymentStatus = master.PaymentStatus });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return ServiceResult.Fail("Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message));
        }
    }

    public ServiceResult VerifyFinal(int masterInvoiceId, string code, int customerId)
    {
        using var ctx = new CarShowroomContext();
        using var tx = ctx.Database.BeginTransaction();
        try
        {
            var master = ctx.MasterInvoices.SingleOrDefault(m => m.MasterInvoiceId == masterInvoiceId);
            if (master == null) return ServiceResult.Fail("Không tìm thấy hóa đơn.");
            if (master.CustomerId != customerId) return ServiceResult.Fail("Bạn không có quyền xác thực hóa đơn này.");
            if (master.IsFinalCaptchaUsed) return ServiceResult.Fail("Hóa đơn đã thanh toán và hoàn tất.");
            if (string.IsNullOrEmpty(master.FinalCaptchaCode)) return ServiceResult.Fail("Chưa có mã tất toán. Vui lòng liên hệ nhân viên.");
            if (master.PurchaseType == "Deposit" && master.PaymentStatus != PaymentStatuses.Deposited)
                return ServiceResult.Fail("Bạn cần xác thực đặt cọc trước khi tất toán.");
            if (!CaptchaHelper.Match(master.FinalCaptchaCode, code))
                return ServiceResult.Fail("Mã xác thực không đúng.");

            var now = DateTime.Now;
            master.IsFinalCaptchaUsed = true;
            master.FinalCaptchaUsedAt = now;
            master.PaymentStatus = PaymentStatuses.Paid;
            master.InvoiceStatus = InvoiceStatuses.Completed;
            master.PaidAt = now;
            master.UpdatedAt = now;
            master.UpdatedUser = customerId;

            ApplyFinalEffects(ctx, master, now);

            ctx.SaveChanges();
            tx.Commit();
            return ServiceResult.Ok("Thanh toán thành công! Cảm ơn quý khách.",
                new { masterInvoiceId, paymentStatus = master.PaymentStatus });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return ServiceResult.Fail("Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message));
        }
    }

    // ---------------------------------------------------------------------
    // Dispatcher: side-effect theo loại hóa đơn con
    // ---------------------------------------------------------------------

    private static void ApplyDepositEffects(CarShowroomContext ctx, MasterInvoice master, DateTime now)
    {
        // Xe: giữ chỗ khi đã đặt cọc.
        foreach (var ci in ctx.CarInvoices.Where(c => c.MasterInvoiceId == master.MasterInvoiceId).ToList())
        {
            var car = ctx.Cars.SingleOrDefault(c => c.CarId == ci.CarId);
            if (car != null && car.Status != "Sold") car.Status = "Reserved";

            if (ci.PurchaseRequestId.HasValue)
            {
                var req = ctx.PurchaseRequests.SingleOrDefault(r => r.RequestId == ci.PurchaseRequestId.Value);
                if (req != null) { req.Status = "Confirmed"; req.UpdatedAt = now; }
            }
        }
        // Phụ tùng & dịch vụ: đặt cọc chưa đổi trạng thái vận hành (giữ nguyên tới khi tất toán).
    }

    private static void ApplyFinalEffects(CarShowroomContext ctx, MasterInvoice master, DateTime now)
    {
        // Xe -> Sold, yêu cầu mua -> Completed.
        foreach (var ci in ctx.CarInvoices.Where(c => c.MasterInvoiceId == master.MasterInvoiceId).ToList())
        {
            var car = ctx.Cars.SingleOrDefault(c => c.CarId == ci.CarId);
            if (car != null) car.Status = "Sold";

            if (ci.PurchaseRequestId.HasValue)
            {
                var req = ctx.PurchaseRequests.SingleOrDefault(r => r.RequestId == ci.PurchaseRequestId.Value);
                if (req != null) { req.Status = "Completed"; req.UpdatedAt = now; }
            }
        }

        // Phụ tùng -> đơn đã thanh toán (Confirmed/Shipping -> giữ nguyên trạng thái giao, chỉ đánh dấu đã trả tiền).
        foreach (var pi in ctx.PartInvoices.Where(p => p.MasterInvoiceId == master.MasterInvoiceId).ToList())
        {
            var order = ctx.PartOrders.SingleOrDefault(o => o.OrderId == pi.PartOrderId);
            if (order != null)
            {
                if (order.Status == "Pending") order.Status = "Confirmed";
                order.UpdatedAt = now;
            }
        }

        // Dịch vụ -> đánh dấu đã thanh toán.
        foreach (var si in ctx.ServiceInvoices.Where(s => s.MasterInvoiceId == master.MasterInvoiceId).ToList())
        {
            var appt = ctx.MaintenanceAppointments.SingleOrDefault(a => a.AppointmentId == si.AppointmentId);
            if (appt != null) { appt.IsPaid = true; appt.UpdatedAt = now; }
        }
    }

    // ---------------------------------------------------------------------
    // Giải phóng cọc hết hạn (mọi module)
    // ---------------------------------------------------------------------

    public int ReleaseExpiredInvoices()
    {
        using var ctx = new CarShowroomContext();
        using var tx = ctx.Database.BeginTransaction();
        var now = DateTime.Now;
        int released = 0;
        try
        {
            // 1. Quá 30 phút chưa xác thực captcha 1 lớp nào
            var expiredUnconfirmed = ctx.MasterInvoices.Where(m =>
                !m.IsDepositCaptchaUsed &&
                !m.IsFinalCaptchaUsed &&
                m.PaymentStatus == PaymentStatuses.Unpaid &&
                m.InvoiceStatus != InvoiceStatuses.Completed &&
                m.InvoiceStatus != InvoiceStatuses.Cancelled &&
                ((m.ExpiredAt != null && m.ExpiredAt < now) || m.CreatedAt.AddMinutes(30) < now)).ToList();

            foreach (var master in expiredUnconfirmed)
            {
                master.InvoiceStatus = InvoiceStatuses.Cancelled;
                master.Notes = (string.IsNullOrEmpty(master.Notes) ? "" : master.Notes + " | ")
                    + $"Tự động hết hiệu lực do quá 30 phút chưa xác thực captcha ({now:dd/MM/yyyy HH:mm}).";
                master.UpdatedAt = now;

                // Hoàn lại xe nếu đang bị khóa Reserved
                foreach (var ci in ctx.CarInvoices.Where(c => c.MasterInvoiceId == master.MasterInvoiceId).ToList())
                {
                    var car = ctx.Cars.SingleOrDefault(c => c.CarId == ci.CarId);
                    if (car != null && car.Status == "Reserved") car.Status = "Available";
                    if (ci.PurchaseRequestId.HasValue)
                    {
                        var req = ctx.PurchaseRequests.SingleOrDefault(r => r.RequestId == ci.PurchaseRequestId.Value);
                        if (req != null && req.Status == "Confirmed") { req.Status = "Pending"; req.UpdatedAt = now; }
                    }
                }

                // Hoàn lại số lượng phụ tùng vào kho nếu đã từng trừ kho
                foreach (var pi in ctx.PartInvoices.Where(p => p.MasterInvoiceId == master.MasterInvoiceId).ToList())
                {
                    var order = ctx.PartOrders.Include(o => o.PartOrderDetails).SingleOrDefault(o => o.OrderId == pi.PartOrderId);
                    if (order != null)
                    {
                        foreach (var detail in order.PartOrderDetails)
                        {
                            var part = ctx.Parts.SingleOrDefault(p => p.PartId == detail.PartId);
                            if (part != null)
                            {
                                part.Quantity += detail.Quantity;
                                if (part.Status == "OutOfStock" && part.Quantity > 0) part.Status = "Available";

                                ctx.InventoryTransactions.Add(new InventoryTransaction
                                {
                                    PartId = part.PartId,
                                    TransactionType = InventoryTransactionTypes.Import,
                                    Quantity = detail.Quantity,
                                    ReferenceType = InventoryReferenceTypes.PartOrder,
                                    ReferenceId = order.OrderId,
                                    StaffId = master.StaffId ?? master.CustomerId,
                                    Notes = $"Hoàn tồn kho do hóa đơn tổng #{master.MasterInvoiceId} hết hạn 30 phút chưa xác thực",
                                    TransactionDate = now,
                                    CreatedAt = now,
                                    CreatedUser = master.CustomerId
                                });
                            }
                        }
                        if (order.Status == "Confirmed") order.Status = "Cancelled";
                    }
                }

                released++;
            }

            // 2. Hóa đơn cọc quá hạn 2 tuần (14 ngày)
            var expiredDeposits = ctx.MasterInvoices.Where(m =>
                m.PurchaseType == "Deposit" &&
                m.IsDepositCaptchaUsed &&
                m.DepositExpiresAt != null && m.DepositExpiresAt < now &&
                m.PaymentStatus != PaymentStatuses.Paid &&
                m.InvoiceStatus != InvoiceStatuses.Completed &&
                m.InvoiceStatus != InvoiceStatuses.Cancelled).ToList();

            foreach (var master in expiredDeposits)
            {
                master.InvoiceStatus = InvoiceStatuses.Cancelled;
                master.Notes = (string.IsNullOrEmpty(master.Notes) ? "" : master.Notes + " | ")
                    + $"Tự động hủy do quá hạn giữ cọc 2 tuần ({now:dd/MM/yyyy}); khách mất tiền đặt cọc.";
                master.UpdatedAt = now;

                foreach (var ci in ctx.CarInvoices.Where(c => c.MasterInvoiceId == master.MasterInvoiceId).ToList())
                {
                    var car = ctx.Cars.SingleOrDefault(c => c.CarId == ci.CarId);
                    if (car != null && car.Status == "Reserved") car.Status = "Available";
                    if (ci.PurchaseRequestId.HasValue)
                    {
                        var req = ctx.PurchaseRequests.SingleOrDefault(r => r.RequestId == ci.PurchaseRequestId.Value);
                        if (req != null && req.Status == "Confirmed") { req.Status = "Rejected"; req.UpdatedAt = now; }
                    }
                }
                released++;
            }

            ctx.SaveChanges();
            tx.Commit();
            return released;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public int ReleaseExpiredDeposits() => ReleaseExpiredInvoices();

    // ---------------------------------------------------------------------
    // Truy vấn hóa đơn tổng (kèm dòng chi tiết mọi module)
    // ---------------------------------------------------------------------

    public MasterInvoiceViewDto? GetInvoice(int masterInvoiceId, bool includeCaptcha)
    {
        using var ctx = new CarShowroomContext();
        var master = ctx.MasterInvoices.AsNoTracking().SingleOrDefault(m => m.MasterInvoiceId == masterInvoiceId);
        if (master == null) return null;
        var customerName = ctx.AppUsers.AsNoTracking()
            .Where(u => u.UserId == master.CustomerId).Select(u => u.FullName).FirstOrDefault() ?? "";
        return Map(ctx, master, customerName, includeCaptcha);
    }

    public IEnumerable<MasterInvoiceViewDto> GetInvoices(int? customerId, string? invoiceType, bool includeCaptcha)
    {
        // Tự động dọn các hóa đơn cọc quá hạn 2 tuần mỗi khi liệt kê (không để lỗi sweep làm hỏng việc xem).
        try { ReleaseExpiredDeposits(); } catch { /* ignore */ }

        using var ctx = new CarShowroomContext();
        IQueryable<MasterInvoice> q = ctx.MasterInvoices.AsNoTracking();
        if (customerId.HasValue) q = q.Where(m => m.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(invoiceType)) q = q.Where(m => m.InvoiceType == invoiceType);

        var masters = q.OrderByDescending(m => m.CreatedAt).ToList();
        var names = ctx.AppUsers.AsNoTracking().ToDictionary(u => u.UserId, u => u.FullName);

        var result = new List<MasterInvoiceViewDto>();
        foreach (var m in masters)
        {
            try
            {
                result.Add(Map(ctx, m, names.TryGetValue(m.CustomerId, out var n) ? n : "", includeCaptcha));
            }
            catch
            {
                // Một hóa đơn lỗi map không được làm trống cả danh sách.
            }
        }
        return result;
    }

    private static MasterInvoiceViewDto Map(CarShowroomContext ctx, MasterInvoice m, string customerName, bool includeCaptcha)
    {
        var now = DateTime.Now;
        var expAt = m.ExpiredAt ?? m.CreatedAt.AddMinutes(30);
        int remSec = (int)Math.Max(0, (expAt - now).TotalSeconds);

        var dto = new MasterInvoiceViewDto
        {
            MasterInvoiceId = m.MasterInvoiceId,
            InvoiceNumber = m.InvoiceNumber,
            InvoiceType = m.InvoiceType,
            CustomerId = m.CustomerId,
            CustomerName = customerName,
            StaffId = m.StaffId,
            PurchaseType = m.PurchaseType ?? "Buyout",
            PaymentStatus = m.PaymentStatus,
            InvoiceStatus = m.InvoiceStatus,
            TotalSubTotal = m.TotalSubTotal,
            DiscountAmount = m.DiscountAmount,
            TaxAmount = m.TaxAmount,
            TotalAmount = m.TotalAmount,
            DepositAmount = m.DepositAmount,
            DepositPaidAmount = m.DepositPaidAmount,
            DepositExpiresAt = m.DepositExpiresAt,
            ExpiredAt = expAt,
            RemainingSeconds = remSec,
            IsDepositCaptchaUsed = m.IsDepositCaptchaUsed,
            IsFinalCaptchaUsed = m.IsFinalCaptchaUsed,
            DepositCaptchaCode = includeCaptcha ? m.DepositCaptchaCode : null,
            FinalCaptchaCode = includeCaptcha ? m.FinalCaptchaCode : null,
            CreatedAt = m.CreatedAt,
            PaidAt = m.PaidAt,
            Notes = m.Notes
        };

        // Dòng xe
        var carLines = from ci in ctx.CarInvoices.AsNoTracking()
                       join car in ctx.Cars.AsNoTracking() on ci.CarId equals car.CarId
                       where ci.MasterInvoiceId == m.MasterInvoiceId
                       select new InvoiceLineDto
                       {
                           ItemType = InvoiceTypes.Car,
                           ReferenceId = ci.CarId,
                           Description = car.CarName,
                           SubTotal = ci.UnitPrice + ci.RegistrationFee + ci.PlateFee + ci.InsuranceFee
                       };
        dto.Lines.AddRange(carLines.ToList());

        // Dòng phụ tùng: liệt kê từng phụ tùng theo tên (thay vì "Đơn phụ tùng #X").
        var partInvs = ctx.PartInvoices.AsNoTracking()
            .Where(pi => pi.MasterInvoiceId == m.MasterInvoiceId)
            .Select(pi => new { pi.PartOrderId, pi.ShippingFee })
            .ToList();
        foreach (var pinv in partInvs)
        {
            var details = (from d in ctx.PartOrderDetails.AsNoTracking()
                           join p in ctx.Parts.AsNoTracking() on d.PartId equals p.PartId
                           where d.OrderId == pinv.PartOrderId
                           select new { p.PartName, d.Quantity, d.UnitPrice }).ToList();
            foreach (var d in details)
            {
                dto.Lines.Add(new InvoiceLineDto
                {
                    ItemType = InvoiceTypes.Part,
                    ReferenceId = pinv.PartOrderId,
                    Description = $"{d.PartName} (x{d.Quantity})",
                    SubTotal = d.UnitPrice * d.Quantity
                });
            }
            if (pinv.ShippingFee > 0)
            {
                dto.Lines.Add(new InvoiceLineDto
                {
                    ItemType = InvoiceTypes.Part,
                    ReferenceId = pinv.PartOrderId,
                    Description = "Phí giao hàng (đơn phụ tùng #" + pinv.PartOrderId + ")",
                    SubTotal = pinv.ShippingFee
                });
            }
        }

        // Dòng dịch vụ: liệt kê gói/dịch vụ + phụ tùng bảo dưỡng theo tên.
        var apptIds = ctx.ServiceInvoices.AsNoTracking()
            .Where(si => si.MasterInvoiceId == m.MasterInvoiceId)
            .Select(si => si.AppointmentId)
            .ToList();
        foreach (var apptId in apptIds)
        {
            var detRows = ctx.AppointmentDetails.AsNoTracking()
                .Where(d => d.AppointmentId == apptId)
                .Select(d => new { d.ServiceId, d.PackageId, d.UnitPrice, d.Quantity })
                .ToList();
            foreach (var d in detRows)
            {
                string name = d.PackageId != null
                    ? (ctx.MaintenancePackages.AsNoTracking().Where(p => p.PackageId == d.PackageId).Select(p => p.PackageName).FirstOrDefault() ?? ("Gói #" + d.PackageId))
                    : (ctx.Services.AsNoTracking().Where(s => s.ServiceId == d.ServiceId).Select(s => s.ServiceName).FirstOrDefault() ?? ("Dịch vụ #" + d.ServiceId));
                dto.Lines.Add(new InvoiceLineDto
                {
                    ItemType = InvoiceTypes.Service,
                    ReferenceId = apptId,
                    Description = name,
                    SubTotal = d.UnitPrice * d.Quantity
                });
            }

            var cpRows = (from cp in ctx.AppointmentConsumedParts.AsNoTracking()
                          join p in ctx.Parts.AsNoTracking() on cp.PartId equals p.PartId
                          where cp.AppointmentId == apptId && (!cp.IsIncurred || cp.ApprovedByCustomer)
                          select new { p.PartName, cp.Quantity, cp.UnitPrice }).ToList();
            foreach (var cp in cpRows)
            {
                dto.Lines.Add(new InvoiceLineDto
                {
                    ItemType = InvoiceTypes.Part,
                    ReferenceId = apptId,
                    Description = cp.PartName + " (x" + cp.Quantity + ", bảo dưỡng)",
                    SubTotal = cp.UnitPrice * cp.Quantity
                });
            }
        }

        return dto;
    }
}
