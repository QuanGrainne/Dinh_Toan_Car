using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class MaintenanceAppointment
{
    public int AppointmentId { get; set; }

    /// <summary>Optional backlink to MasterInvoice if invoiced</summary>
    public int? MasterInvoiceId { get; set; }

    public int CustomerId { get; set; }

    /// <summary>Relies on registered customer car (CustomerCars table)</summary>
    public int CustomerCarId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string? Note { get; set; }

    /// <summary>Pending | Confirmed | InProgress | Completed | Cancelled</summary>
    public string Status { get; set; } = null!;
    
    public bool IsPaid { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual AppUser Customer { get; set; } = null!;
    public virtual CustomerCar CustomerCar { get; set; } = null!;
    public virtual ICollection<AppointmentDetail> AppointmentDetails { get; set; } = new List<AppointmentDetail>();
    public virtual ICollection<AppointmentConsumedPart> ConsumedParts { get; set; } = new List<AppointmentConsumedPart>();
}
