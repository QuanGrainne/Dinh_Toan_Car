using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class PartOrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? SubTotal { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual PartOrder Order { get; set; } = null!;

    public virtual Part Part { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
