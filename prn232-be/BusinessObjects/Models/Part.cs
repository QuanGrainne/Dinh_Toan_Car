using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class Part
    {
        public int PartId { get; set; }

        public int CategoryId { get; set; }

        public string PartName { get; set; } = null!;

        public string PartCode { get; set; } = null!;

        public string? Brand { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int MinStockLevel { get; set; } = 5;

        public int MaxStockLevel { get; set; } = 100;

        public string UnitOfMeasure { get; set; } = "Cái";

        public string? WarehouseLocation { get; set; }

        public int WarrantyMonths { get; set; } = 0;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string Status { get; set; } = "Available";

        public DateTime? ExpiredAt { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedUser { get; set; }

        public virtual PartCategory Category { get; set; } = null!;
        public virtual AppUser? CreatedUserNavigation { get; set; }
        public virtual AppUser? UpdatedUserNavigation { get; set; }

        public virtual ICollection<PartOrderDetail> PartOrderDetails { get; set; } = new List<PartOrderDetail>();
        public virtual ICollection<PartCompatibility> PartCompatibilities { get; set; } = new List<PartCompatibility>();
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetails { get; set; } = new List<InventoryReceiptDetail>();

        public virtual ICollection<ServiceRequiredPart> ServiceRequiredParts { get; set; } = new List<ServiceRequiredPart>();

        public virtual ICollection<AppointmentConsumedPart> AppointmentConsumedParts { get; set; } = new List<AppointmentConsumedPart>();
    }
}