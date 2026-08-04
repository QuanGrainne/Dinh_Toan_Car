using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class AppRole
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
