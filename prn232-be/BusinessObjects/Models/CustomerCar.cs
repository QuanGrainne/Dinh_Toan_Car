using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Xe của khách hàng đã đăng ký vào hệ thống (dùng khi đặt lịch bảo dưỡng)
/// </summary>
public partial class CustomerCar
{
    public int CustomerCarId { get; set; }

    public int CustomerId { get; set; }

    public int BrandId { get; set; }

    public string Model { get; set; } = null!;

    public int? Year { get; set; }

    public string? VIN { get; set; }

    public string LicensePlate { get; set; } = null!;

    public string? Color { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual AppUser Customer { get; set; } = null!;
    public virtual CarBrand Brand { get; set; } = null!;
    public virtual ICollection<MaintenanceAppointment> MaintenanceAppointments { get; set; } = new List<MaintenanceAppointment>();
}
