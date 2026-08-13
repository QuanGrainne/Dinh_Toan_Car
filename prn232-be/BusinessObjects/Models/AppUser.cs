using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class AppUser
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? VerificationCode { get; set; }

    public DateTime? CodeExpiryTime { get; set; }

    public virtual AppRole Role { get; set; } = null!;
}
