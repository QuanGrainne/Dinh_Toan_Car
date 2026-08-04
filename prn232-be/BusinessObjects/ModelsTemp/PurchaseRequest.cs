using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class PurchaseRequest
{
    public int RequestId { get; set; }

    public int CarId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public string? Message { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual ICollection<CarInvoice> CarInvoices { get; set; } = new List<CarInvoice>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual AppUser Customer { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
