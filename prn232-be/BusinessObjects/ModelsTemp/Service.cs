using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public int EstimatedDurationMinutes { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual ICollection<AppointmentDetail> AppointmentDetails { get; set; } = new List<AppointmentDetail>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();

    public virtual ICollection<ServiceRequiredPart> ServiceRequiredParts { get; set; } = new List<ServiceRequiredPart>();

    public virtual ICollection<ServiceStaffAssignment> ServiceStaffAssignments { get; set; } = new List<ServiceStaffAssignment>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
