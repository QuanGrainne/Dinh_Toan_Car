using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class PartInvoice
{
    public int PartInvoiceId { get; set; }

    public int MasterInvoiceId { get; set; }

    public int PartOrderId { get; set; }

    public decimal SubTotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual MasterInvoice MasterInvoice { get; set; } = null!;

    public virtual PartOrder PartOrder { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
