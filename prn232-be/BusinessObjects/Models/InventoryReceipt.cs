using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class InventoryReceipt
    {
        public int ReceiptId { get; set; }
        public int SupplierId { get; set; }
        public int StaffId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedUser { get; set; }

        public virtual Supplier Supplier { get; set; } = null!;
        public virtual AppUser Staff { get; set; } = null!;
        public virtual AppUser? CreatedUserNavigation { get; set; }
        public virtual AppUser? UpdatedUserNavigation { get; set; }

        public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetails { get; set; } = new List<InventoryReceiptDetail>();
    }
}
