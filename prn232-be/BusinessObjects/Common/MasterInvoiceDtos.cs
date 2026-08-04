using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Common;

/// <summary>Loại hóa đơn (dùng cho cột MasterInvoices.InvoiceType).</summary>
public static class InvoiceTypesExtra
{
    public const string Combined = "Combined"; // Hóa đơn gộp nhiều module
}

/// <summary>Một dòng chi tiết trong hóa đơn tổng (xe / phụ tùng / dịch vụ).</summary>
public class InvoiceLineDto
{
    public string ItemType { get; set; } = null!; // Car | Part | Service
    public int ReferenceId { get; set; }          // CarId | PartOrderId | AppointmentId
    public string Description { get; set; } = null!;
    public decimal SubTotal { get; set; }
}

/// <summary>Chi tiết hóa đơn tổng, dùng chung cho cả 3 module và hóa đơn gộp.</summary>
public class MasterInvoiceViewDto
{
    public int MasterInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public string InvoiceType { get; set; } = null!; // Car | Part | Service | Combined
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public int? StaffId { get; set; }

    public string PurchaseType { get; set; } = null!; // Deposit | Buyout
    public string PaymentStatus { get; set; } = null!;
    public string InvoiceStatus { get; set; } = null!;

    public decimal TotalSubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal? DepositAmount { get; set; }
    public decimal? DepositPaidAmount { get; set; }
    public DateTime? DepositExpiresAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public int RemainingSeconds { get; set; }
    public bool IsDepositCaptchaUsed { get; set; }
    public bool IsFinalCaptchaUsed { get; set; }

    /// <summary>Chỉ trả cho Admin/Staff; với khách hàng luôn null.</summary>
    public string? DepositCaptchaCode { get; set; }
    /// <summary>Chỉ trả cho Admin/Staff; với khách hàng luôn null.</summary>
    public string? FinalCaptchaCode { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }

    public List<InvoiceLineDto> Lines { get; set; } = new();
}

/// <summary>Nhân viên sinh mã đặt cọc cho một hóa đơn tổng (bất kỳ module nào).</summary>
public class GenerateDepositCaptchaDto
{
    [Range(1, int.MaxValue)]
    public int MasterInvoiceId { get; set; }

    /// <summary>Số tiền cọc; bỏ trống => mặc định 10% tổng tiền.</summary>
    [Range(0, 100000000000)]
    public decimal? DepositAmount { get; set; }

    [Range(1, 365)]
    public int DepositExpiresInDays { get; set; } = 7;
}

// =====================================================================
// CHECKOUT (tạo 1 MasterInvoice cho mua lẻ hoặc mua gộp nhiều module)
// =====================================================================

/// <summary>Một dòng xe trong checkout (tham chiếu yêu cầu mua đã tạo + các phí kèm theo).</summary>
public class CheckoutCarLineDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã yêu cầu mua xe không hợp lệ.")]
    public int PurchaseRequestId { get; set; }

    [Range(0, 100000000000)] public decimal RegistrationFee { get; set; }
    [Range(0, 100000000000)] public decimal PlateFee { get; set; }
    [Range(0, 100000000000)] public decimal InsuranceFee { get; set; }
}

/// <summary>
/// Yêu cầu tạo hóa đơn tổng. Nhân viên gộp bất kỳ tổ hợp: xe + đơn phụ tùng + lịch dịch vụ
/// của CÙNG một khách hàng vào MỘT master. PurchaseType = Deposit (đặt cọc) hoặc Buyout (mua đứt).
/// </summary>
public class CheckoutDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Thiếu mã khách hàng.")]
    public int CustomerId { get; set; }

    [Required]
    [RegularExpression("Deposit|Buyout", ErrorMessage = "PurchaseType chỉ nhận 'Deposit' hoặc 'Buyout'.")]
    public string PurchaseType { get; set; } = "Buyout";

    public List<CheckoutCarLineDto> Cars { get; set; } = new();
    public List<int> PartOrderIds { get; set; } = new();
    public List<int> AppointmentIds { get; set; } = new();

    [Range(0, 100000000000)] public decimal DiscountAmount { get; set; }
    [Range(0, 100000000000)] public decimal TaxAmount { get; set; }

    /// <summary>Chỉ dùng khi PurchaseType = Deposit. Bỏ trống => mặc định 10% tổng tiền.</summary>
    [Range(0, 100000000000)]
    public decimal? DepositAmount { get; set; }

    [Range(1, 365)]
    public int DepositExpiresInDays { get; set; } = 7;

    [StringLength(1000)]
    public string? Notes { get; set; }
}
