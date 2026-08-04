using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class AppUser
    {
        public virtual ICollection<Supplier> CreatedSuppliers { get; set; } = new List<Supplier>();
        public virtual ICollection<Supplier> UpdatedSuppliers { get; set; } = new List<Supplier>();

        public virtual ICollection<Part> CreatedParts { get; set; } = new List<Part>();
        public virtual ICollection<Part> UpdatedParts { get; set; } = new List<Part>();

        public virtual ICollection<PartCategory> CreatedPartCategories { get; set; } = new List<PartCategory>();
        public virtual ICollection<PartCategory> UpdatedPartCategories { get; set; } = new List<PartCategory>();

        public virtual ICollection<PartCompatibility> CreatedPartCompatibilities { get; set; } = new List<PartCompatibility>();
        public virtual ICollection<PartCompatibility> UpdatedPartCompatibilities { get; set; } = new List<PartCompatibility>();

        public virtual ICollection<InventoryReceipt> StaffReceipts { get; set; } = new List<InventoryReceipt>();
        public virtual ICollection<InventoryReceipt> CreatedReceipts { get; set; } = new List<InventoryReceipt>();
        public virtual ICollection<InventoryReceipt> UpdatedReceipts { get; set; } = new List<InventoryReceipt>();

        public virtual ICollection<InventoryReceiptDetail> CreatedReceiptDetails { get; set; } = new List<InventoryReceiptDetail>();
        public virtual ICollection<InventoryReceiptDetail> UpdatedReceiptDetails { get; set; } = new List<InventoryReceiptDetail>();

        public virtual ICollection<InventoryTransaction> StaffTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<InventoryTransaction> CreatedTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<InventoryTransaction> UpdatedTransactions { get; set; } = new List<InventoryTransaction>();
    }
}
