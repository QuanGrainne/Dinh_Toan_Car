using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class CarInvoice
{
    public int CarInvoiceId { get; set; }

    public int MasterInvoiceId { get; set; }

    public int CarId { get; set; }

    public int? PurchaseRequestId { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal RegistrationFee { get; set; }

    public decimal PlateFee { get; set; }

    public decimal InsuranceFee { get; set; }

    public decimal? SubTotal { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual MasterInvoice MasterInvoice { get; set; } = null!;

    public virtual PurchaseRequest? PurchaseRequest { get; set; }

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
