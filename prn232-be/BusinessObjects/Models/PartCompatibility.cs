using System;

namespace BusinessObjects.Models
{
    public partial class PartCompatibility
    {
        public int CompatibilityId { get; set; }
        public int PartId { get; set; }
        public int BrandId { get; set; }
        public string ModelName { get; set; } = null!;
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CreatedUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedUser { get; set; }

        public virtual Part Part { get; set; } = null!;
        public virtual CarBrand Brand { get; set; } = null!;
        public virtual AppUser? CreatedUserNavigation { get; set; }
        public virtual AppUser? UpdatedUserNavigation { get; set; }
    }
}
