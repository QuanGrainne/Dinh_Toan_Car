using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class PartOrder
{
    public int OrderId { get; set; }

    public int? MasterInvoiceId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public string? ShippingAddress { get; set; }

    public string DeliveryMethod { get; set; } = null!;

    public decimal ShippingFee { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual AppUser Customer { get; set; } = null!;

    public virtual MasterInvoice? MasterInvoice { get; set; }

    public virtual PartInvoice? PartInvoice { get; set; }

    public virtual ICollection<PartOrderDetail> PartOrderDetails { get; set; } = new List<PartOrderDetail>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
