using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class MasterInvoice
{
    public int MasterInvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public string InvoiceType { get; set; } = "Car"; // Car, Part, Service

    public int CustomerId { get; set; }

    public int? StaffId { get; set; }

    public decimal TotalSubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PurchaseType { get; set; } // Deposit, Buyout (for Car)

    public string PaymentStatus { get; set; } = "Unpaid";

    public string InvoiceStatus { get; set; } = "Pending";

    public string? PaymentMethod { get; set; } // CashAtShowroom, BankTransfer, COD

    public string? PaymentReference { get; set; }

    public DateTime? PaidAt { get; set; }

    public decimal? DepositAmount { get; set; }

    public decimal? DepositPaidAmount { get; set; }

    public DateTime? DepositExpiresAt { get; set; }

    public string? DepositCaptchaCode { get; set; }

    public bool IsDepositCaptchaUsed { get; set; }

    public DateTime? DepositCaptchaUsedAt { get; set; }

    public string? FinalCaptchaCode { get; set; }

    public bool IsFinalCaptchaUsed { get; set; }

    public DateTime? FinalCaptchaUsedAt { get; set; }

    public string? Notes { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual ICollection<CarInvoice> CarInvoices { get; set; } = new List<CarInvoice>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual AppUser Customer { get; set; } = null!;

    public virtual ICollection<MaintenanceAppointment> MaintenanceAppointments { get; set; } = new List<MaintenanceAppointment>();

    public virtual ICollection<PartInvoice> PartInvoices { get; set; } = new List<PartInvoice>();

    public virtual ICollection<PartOrder> PartOrders { get; set; } = new List<PartOrder>();

    public virtual ICollection<ServiceInvoice> ServiceInvoices { get; set; } = new List<ServiceInvoice>();

    public virtual AppUser? Staff { get; set; }

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
