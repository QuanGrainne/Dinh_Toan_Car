using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class PartOrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal SubTotal { get; set; }

    public virtual PartOrder Order { get; set; } = null!;

    public virtual Part Part { get; set; } = null!;
}
