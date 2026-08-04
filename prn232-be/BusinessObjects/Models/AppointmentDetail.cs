using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Chi tiết lịch hẹn bảo dưỡng: chứa thông tin từng gói/dịch vụ được chọn (với giá khóa tại thời điểm đặt)
/// </summary>
public partial class AppointmentDetail
{
    public int AppointmentDetailId { get; set; }

    public int AppointmentId { get; set; }

    /// <summary>Null nếu khách chọn dịch vụ lẻ</summary>
    public int? PackageId { get; set; }

    /// <summary>Null nếu khách chọn gói</summary>
    public int? ServiceId { get; set; }

    /// <summary>Giá khóa tại thời điểm đặt lịch</summary>
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual MaintenanceAppointment Appointment { get; set; } = null!;
    public virtual MaintenancePackage? Package { get; set; }
    public virtual Service? Service { get; set; }
    public virtual ICollection<AppointmentConsumedPart> ConsumedParts { get; set; } = new List<AppointmentConsumedPart>();
}
