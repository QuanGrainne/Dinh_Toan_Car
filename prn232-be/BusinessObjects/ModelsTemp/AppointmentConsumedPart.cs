using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class AppointmentConsumedPart
{
    public int ConsumedPartId { get; set; }

    public int AppointmentId { get; set; }

    public int? AppointmentDetailId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsIncurred { get; set; }

    public bool ApprovedByCustomer { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual MaintenanceAppointment Appointment { get; set; } = null!;

    public virtual AppointmentDetail? AppointmentDetail { get; set; }

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual Part Part { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
