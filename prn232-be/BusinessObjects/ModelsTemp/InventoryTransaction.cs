using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class InventoryTransaction
{
    public int TransactionId { get; set; }

    public int PartId { get; set; }

    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public int StaffId { get; set; }

    public string? Notes { get; set; }

    public DateTime TransactionDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual Part Part { get; set; } = null!;

    public virtual AppUser Staff { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
