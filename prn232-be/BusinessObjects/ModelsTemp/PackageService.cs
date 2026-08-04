using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class PackageService
{
    public int PackageId { get; set; }

    public int ServiceId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual MaintenancePackage Package { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
