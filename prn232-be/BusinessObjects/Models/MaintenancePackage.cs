using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessObjects.Models;

public partial class MaintenancePackage
{
    public int PackageId { get; set; }

    public string PackageName { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>Giá combo có thể ưu đãi so với tổng giá từng dịch vụ lẻ</summary>
    public decimal PackagePrice { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();
    public virtual ICollection<AppointmentDetail> AppointmentDetails { get; set; } = new List<AppointmentDetail>();

    public int TotalDurationMinutes => PackageServices?.Sum(ps => ps.Service?.EstimatedDurationMinutes ?? 0) ?? 0;
}
