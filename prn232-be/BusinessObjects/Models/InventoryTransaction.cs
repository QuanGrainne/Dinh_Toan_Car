using System;

namespace BusinessObjects.Models
{
    public partial class InventoryTransaction
    {
        public int TransactionId { get; set; }
        public int PartId { get; set; }
        public string TransactionType { get; set; } = null!; // Import, Export, Return, Adjustment
        public int Quantity { get; set; }
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public int StaffId { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedUser { get; set; }

        public virtual Part Part { get; set; } = null!;
        public virtual AppUser Staff { get; set; } = null!;
        public virtual AppUser? CreatedUserNavigation { get; set; }
        public virtual AppUser? UpdatedUserNavigation { get; set; }
    }
}
