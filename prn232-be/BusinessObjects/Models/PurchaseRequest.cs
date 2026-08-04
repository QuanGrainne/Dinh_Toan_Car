using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Yêu cầu mua xe của khách hàng (module ô tô).
/// Ánh xạ 1-1 với bảng PurchaseRequests trong CarShowroomDB v2.
/// Nghiệp vụ đặt cọc / mua đứt được xác thực qua MasterInvoice (mã captcha do nhân viên tạo).
/// </summary>
public partial class PurchaseRequest
{
    public int RequestId { get; set; }

    public int CarId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public string? Message { get; set; }

    /// <summary>Pending | Confirmed | Rejected | Completed (theo CK_PurchaseRequests_Status của v2).</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Thời hạn giữ chỗ (ví dụ hết hạn đặt cọc).</summary>
    public DateTime? ExpiredAt { get; set; }

    // Audit fields (v2)
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    // Navigation
    public virtual Car Car { get; set; } = null!;

    public virtual AppUser Customer { get; set; } = null!;
}
