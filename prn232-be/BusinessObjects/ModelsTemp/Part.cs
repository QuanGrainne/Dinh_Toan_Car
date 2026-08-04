using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class Part
{
    public int PartId { get; set; }

    public int CategoryId { get; set; }

    public string PartName { get; set; } = null!;

    public string PartCode { get; set; } = null!;

    public string? Brand { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int MinStockLevel { get; set; }

    public int MaxStockLevel { get; set; }

    public string UnitOfMeasure { get; set; } = null!;

    public string? WarehouseLocation { get; set; }

    public int WarrantyMonths { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual ICollection<AppointmentConsumedPart> AppointmentConsumedParts { get; set; } = new List<AppointmentConsumedPart>();

    public virtual PartCategory Category { get; set; } = null!;

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetails { get; set; } = new List<InventoryReceiptDetail>();

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<PartCompatibility> PartCompatibilities { get; set; } = new List<PartCompatibility>();

    public virtual ICollection<PartOrderDetail> PartOrderDetails { get; set; } = new List<PartOrderDetail>();

    public virtual ICollection<ServiceRequiredPart> ServiceRequiredParts { get; set; } = new List<ServiceRequiredPart>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
