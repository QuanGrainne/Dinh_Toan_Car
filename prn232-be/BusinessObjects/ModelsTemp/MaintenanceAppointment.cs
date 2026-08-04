using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

public partial class MaintenanceAppointment
{
    public int AppointmentId { get; set; }

    public int? MasterInvoiceId { get; set; }

    public int CustomerId { get; set; }

    public int CustomerCarId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual ICollection<AppointmentConsumedPart> AppointmentConsumedParts { get; set; } = new List<AppointmentConsumedPart>();

    public virtual ICollection<AppointmentDetail> AppointmentDetails { get; set; } = new List<AppointmentDetail>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual AppUser Customer { get; set; } = null!;

    public virtual CustomerCar CustomerCar { get; set; } = null!;

    public virtual MasterInvoice? MasterInvoice { get; set; }

    public virtual ServiceInvoice? ServiceInvoice { get; set; }

    public virtual ICollection<ServiceStaffAssignment> ServiceStaffAssignments { get; set; } = new List<ServiceStaffAssignment>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
