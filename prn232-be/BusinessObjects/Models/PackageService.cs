using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class PackageService
{
    public int PackageId { get; set; }

    public int ServiceId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual MaintenancePackage Package { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
