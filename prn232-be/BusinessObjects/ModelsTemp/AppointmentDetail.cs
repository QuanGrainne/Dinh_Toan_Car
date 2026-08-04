using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class AppointmentDetail
{
    public int AppointmentDetailId { get; set; }

    public int AppointmentId { get; set; }

    public int? PackageId { get; set; }

    public int? ServiceId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal? SubTotal { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual MaintenanceAppointment Appointment { get; set; } = null!;

    public virtual ICollection<AppointmentConsumedPart> AppointmentConsumedParts { get; set; } = new List<AppointmentConsumedPart>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual MaintenancePackage? Package { get; set; }

    public virtual Service? Service { get; set; }

    public virtual ICollection<ServiceExecutionLog> ServiceExecutionLogs { get; set; } = new List<ServiceExecutionLog>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
