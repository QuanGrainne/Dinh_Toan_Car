using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Phụ tùng định mức tiêu chuẩn cho một dịch vụ (e.g. Thay dầu cần 4L dầu Castrol)
/// </summary>
public partial class ServiceRequiredPart
{
    public int ServiceId { get; set; }

    public int PartId { get; set; }

    public int QuantityRequired { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Service Service { get; set; } = null!;
    public virtual Part Part { get; set; } = null!;
}
