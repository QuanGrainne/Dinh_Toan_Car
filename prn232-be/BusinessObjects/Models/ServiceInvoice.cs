using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ServiceInvoice
{
    public int ServiceInvoiceId { get; set; }

    public int MasterInvoiceId { get; set; }

    public int AppointmentId { get; set; }

    public decimal SubTotal { get; set; }

    public decimal LaborDiscount { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual MaintenanceAppointment Appointment { get; set; } = null!;

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual MasterInvoice MasterInvoice { get; set; } = null!;

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
