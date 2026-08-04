using System;

namespace BusinessObjects.Models
{
    public partial class InventoryReceiptDetail
    {
        public int ReceiptDetailId { get; set; }
        public int ReceiptId { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SubTotal { get; set; } // Computed column in DB

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedUser { get; set; }

        public virtual InventoryReceipt Receipt { get; set; } = null!;
        public virtual Part Part { get; set; } = null!;
        public virtual AppUser? CreatedUserNavigation { get; set; }
        public virtual AppUser? UpdatedUserNavigation { get; set; }
    }
}
