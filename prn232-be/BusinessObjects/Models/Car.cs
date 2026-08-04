using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Car
{
    public int CarId { get; set; }

    public int BrandId { get; set; }

    public string CarName { get; set; } = null!;

    public string? Model { get; set; }

    public int Year { get; set; }

    public string? Color { get; set; }

    public int Mileage { get; set; }

    public string FuelType { get; set; } = null!;

    public string Transmission { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual CarBrand Brand { get; set; } = null!;

    public virtual ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
}
