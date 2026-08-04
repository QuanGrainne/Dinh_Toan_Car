using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class ServiceRequiredPart
{
    public int ServiceId { get; set; }

    public int PartId { get; set; }

    public int QuantityRequired { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual Part Part { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
