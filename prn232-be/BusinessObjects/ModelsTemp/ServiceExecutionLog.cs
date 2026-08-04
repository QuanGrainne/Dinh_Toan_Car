using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class ServiceExecutionLog
{
    public int LogId { get; set; }

    public int AppointmentDetailId { get; set; }

    public int StaffId { get; set; }

    public string LogStatus { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime RecordedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual AppointmentDetail AppointmentDetail { get; set; } = null!;

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual AppUser Staff { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
