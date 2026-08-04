using System;

namespace BusinessObjects.Models;

/// <summary>
/// Phụ tùng tiêu thụ trong một buổi bảo dưỡng (bao gồm định mức chuẩn và phụ tùng phát sinh)
/// </summary>
public partial class AppointmentConsumedPart
{
    public int ConsumedPartId { get; set; }

    public int AppointmentId { get; set; }

    /// <summary>Liên kết dịch vụ đang thực hiện (nullable)</summary>
    public int? AppointmentDetailId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    /// <summary>Giá bán phụ tùng tại thời điểm lắp đặt (khóa giá)</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>0 = Định mức chuẩn, 1 = Phụ tùng phát sinh thêm</summary>
    public bool IsIncurred { get; set; }

    /// <summary>true = Đã được khách phê duyệt, false = Chờ duyệt (chỉ áp dụng khi IsIncurred=true)</summary>
    public bool ApprovedByCustomer { get; set; }

    /// <summary>Lý do thay thế phát sinh (e.g. "Má phanh đã mòn trơ cốt thép")</summary>
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual MaintenanceAppointment Appointment { get; set; } = null!;
    public virtual AppointmentDetail? AppointmentDetail { get; set; }
    public virtual Part Part { get; set; } = null!;
}
