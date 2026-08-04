using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class MaintenancePackage
{
    public int PackageId { get; set; }

    public string PackageName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal PackagePrice { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual ICollection<AppointmentDetail> AppointmentDetails { get; set; } = new List<AppointmentDetail>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
