using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class CustomerCar
{
    public int CustomerCarId { get; set; }

    public int CustomerId { get; set; }

    public int BrandId { get; set; }

    public string Model { get; set; } = null!;

    public int? Year { get; set; }

    public string? Vin { get; set; }

    public string LicensePlate { get; set; } = null!;

    public string? Color { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual CarBrand Brand { get; set; } = null!;

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual AppUser Customer { get; set; } = null!;

    public virtual ICollection<MaintenanceAppointment> MaintenanceAppointments { get; set; } = new List<MaintenanceAppointment>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
