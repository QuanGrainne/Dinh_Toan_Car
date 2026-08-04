using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class CarBrand
{
    public int BrandId { get; set; }

    public string BrandName { get; set; } = null!;

    public string? Country { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual ICollection<CustomerCar> CustomerCars { get; set; } = new List<CustomerCar>();

    public virtual ICollection<PartCompatibility> PartCompatibilities { get; set; } = new List<PartCompatibility>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
