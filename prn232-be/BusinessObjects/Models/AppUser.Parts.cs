using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class AppUser
    {
        public virtual ICollection<Part> CreatedParts { get; set; } = new List<Part>();
        public virtual ICollection<Part> UpdatedParts { get; set; } = new List<Part>();

        public virtual ICollection<PartCategory> CreatedPartCategories { get; set; } = new List<PartCategory>();
        public virtual ICollection<PartCategory> UpdatedPartCategories { get; set; } = new List<PartCategory>();
    }
}
