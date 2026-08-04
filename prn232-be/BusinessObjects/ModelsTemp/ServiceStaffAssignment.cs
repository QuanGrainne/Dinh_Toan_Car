using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class ServiceStaffAssignment
{
    public int AssignmentId { get; set; }

    public int AppointmentId { get; set; }

    public int ServiceId { get; set; }

    public int StaffId { get; set; }

    public DateTime AssignedAt { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual MaintenanceAppointment Appointment { get; set; } = null!;

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual Service Service { get; set; } = null!;

    public virtual AppUser Staff { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
