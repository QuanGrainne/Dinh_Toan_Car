using System;
using System.Collections.Generic;

namespace BusinessObjects.ModelsTemp;

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

    public string? VerificationCode { get; set; }

    public DateTime? CodeExpiryTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedUser { get; set; }

    public virtual ICollection<AppRole> AppRoleCreatedUserNavigations { get; set; } = new List<AppRole>();

    public virtual ICollection<AppRole> AppRoleUpdatedUserNavigations { get; set; } = new List<AppRole>();

    public virtual ICollection<AppointmentConsumedPart> AppointmentConsumedPartCreatedUserNavigations { get; set; } = new List<AppointmentConsumedPart>();

    public virtual ICollection<AppointmentConsumedPart> AppointmentConsumedPartUpdatedUserNavigations { get; set; } = new List<AppointmentConsumedPart>();

    public virtual ICollection<AppointmentDetail> AppointmentDetailCreatedUserNavigations { get; set; } = new List<AppointmentDetail>();

    public virtual ICollection<AppointmentDetail> AppointmentDetailUpdatedUserNavigations { get; set; } = new List<AppointmentDetail>();

    public virtual ICollection<CarBrand> CarBrandCreatedUserNavigations { get; set; } = new List<CarBrand>();

    public virtual ICollection<CarBrand> CarBrandUpdatedUserNavigations { get; set; } = new List<CarBrand>();

    public virtual ICollection<Car> CarCreatedUserNavigations { get; set; } = new List<Car>();

    public virtual ICollection<CarInvoice> CarInvoiceCreatedUserNavigations { get; set; } = new List<CarInvoice>();

    public virtual ICollection<CarInvoice> CarInvoiceUpdatedUserNavigations { get; set; } = new List<CarInvoice>();

    public virtual ICollection<Car> CarUpdatedUserNavigations { get; set; } = new List<Car>();

    public virtual AppUser? CreatedUserNavigation { get; set; }

    public virtual ICollection<CustomerCar> CustomerCarCreatedUserNavigations { get; set; } = new List<CustomerCar>();

    public virtual ICollection<CustomerCar> CustomerCarCustomers { get; set; } = new List<CustomerCar>();

    public virtual ICollection<CustomerCar> CustomerCarUpdatedUserNavigations { get; set; } = new List<CustomerCar>();

    public virtual ICollection<InventoryReceipt> InventoryReceiptCreatedUserNavigations { get; set; } = new List<InventoryReceipt>();

    public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetailCreatedUserNavigations { get; set; } = new List<InventoryReceiptDetail>();

    public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetailUpdatedUserNavigations { get; set; } = new List<InventoryReceiptDetail>();

    public virtual ICollection<InventoryReceipt> InventoryReceiptStaffs { get; set; } = new List<InventoryReceipt>();

    public virtual ICollection<InventoryReceipt> InventoryReceiptUpdatedUserNavigations { get; set; } = new List<InventoryReceipt>();

    public virtual ICollection<InventoryTransaction> InventoryTransactionCreatedUserNavigations { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<InventoryTransaction> InventoryTransactionStaffs { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<InventoryTransaction> InventoryTransactionUpdatedUserNavigations { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<AppUser> InverseCreatedUserNavigation { get; set; } = new List<AppUser>();

    public virtual ICollection<AppUser> InverseUpdatedUserNavigation { get; set; } = new List<AppUser>();

    public virtual ICollection<MaintenanceAppointment> MaintenanceAppointmentCreatedUserNavigations { get; set; } = new List<MaintenanceAppointment>();

    public virtual ICollection<MaintenanceAppointment> MaintenanceAppointmentCustomers { get; set; } = new List<MaintenanceAppointment>();

    public virtual ICollection<MaintenanceAppointment> MaintenanceAppointmentUpdatedUserNavigations { get; set; } = new List<MaintenanceAppointment>();

    public virtual ICollection<MaintenancePackage> MaintenancePackageCreatedUserNavigations { get; set; } = new List<MaintenancePackage>();

    public virtual ICollection<MaintenancePackage> MaintenancePackageUpdatedUserNavigations { get; set; } = new List<MaintenancePackage>();

    public virtual ICollection<MasterInvoice> MasterInvoiceCreatedUserNavigations { get; set; } = new List<MasterInvoice>();

    public virtual ICollection<MasterInvoice> MasterInvoiceCustomers { get; set; } = new List<MasterInvoice>();

    public virtual ICollection<MasterInvoice> MasterInvoiceStaffs { get; set; } = new List<MasterInvoice>();

    public virtual ICollection<MasterInvoice> MasterInvoiceUpdatedUserNavigations { get; set; } = new List<MasterInvoice>();

    public virtual ICollection<PackageService> PackageServiceCreatedUserNavigations { get; set; } = new List<PackageService>();

    public virtual ICollection<PackageService> PackageServiceUpdatedUserNavigations { get; set; } = new List<PackageService>();

    public virtual ICollection<PartCategory> PartCategoryCreatedUserNavigations { get; set; } = new List<PartCategory>();

    public virtual ICollection<PartCategory> PartCategoryUpdatedUserNavigations { get; set; } = new List<PartCategory>();

    public virtual ICollection<PartCompatibility> PartCompatibilityCreatedUserNavigations { get; set; } = new List<PartCompatibility>();

    public virtual ICollection<PartCompatibility> PartCompatibilityUpdatedUserNavigations { get; set; } = new List<PartCompatibility>();

    public virtual ICollection<Part> PartCreatedUserNavigations { get; set; } = new List<Part>();

    public virtual ICollection<PartInvoice> PartInvoiceCreatedUserNavigations { get; set; } = new List<PartInvoice>();

    public virtual ICollection<PartInvoice> PartInvoiceUpdatedUserNavigations { get; set; } = new List<PartInvoice>();

    public virtual ICollection<PartOrder> PartOrderCreatedUserNavigations { get; set; } = new List<PartOrder>();

    public virtual ICollection<PartOrder> PartOrderCustomers { get; set; } = new List<PartOrder>();

    public virtual ICollection<PartOrderDetail> PartOrderDetailCreatedUserNavigations { get; set; } = new List<PartOrderDetail>();

    public virtual ICollection<PartOrderDetail> PartOrderDetailUpdatedUserNavigations { get; set; } = new List<PartOrderDetail>();

    public virtual ICollection<PartOrder> PartOrderUpdatedUserNavigations { get; set; } = new List<PartOrder>();

    public virtual ICollection<Part> PartUpdatedUserNavigations { get; set; } = new List<Part>();

    public virtual ICollection<PurchaseRequest> PurchaseRequestCreatedUserNavigations { get; set; } = new List<PurchaseRequest>();

    public virtual ICollection<PurchaseRequest> PurchaseRequestCustomers { get; set; } = new List<PurchaseRequest>();

    public virtual ICollection<PurchaseRequest> PurchaseRequestUpdatedUserNavigations { get; set; } = new List<PurchaseRequest>();

    public virtual AppRole Role { get; set; } = null!;

    public virtual ICollection<Service> ServiceCreatedUserNavigations { get; set; } = new List<Service>();

    public virtual ICollection<ServiceExecutionLog> ServiceExecutionLogCreatedUserNavigations { get; set; } = new List<ServiceExecutionLog>();

    public virtual ICollection<ServiceExecutionLog> ServiceExecutionLogStaffs { get; set; } = new List<ServiceExecutionLog>();

    public virtual ICollection<ServiceExecutionLog> ServiceExecutionLogUpdatedUserNavigations { get; set; } = new List<ServiceExecutionLog>();

    public virtual ICollection<ServiceInvoice> ServiceInvoiceCreatedUserNavigations { get; set; } = new List<ServiceInvoice>();

    public virtual ICollection<ServiceInvoice> ServiceInvoiceUpdatedUserNavigations { get; set; } = new List<ServiceInvoice>();

    public virtual ICollection<ServiceRequiredPart> ServiceRequiredPartCreatedUserNavigations { get; set; } = new List<ServiceRequiredPart>();

    public virtual ICollection<ServiceRequiredPart> ServiceRequiredPartUpdatedUserNavigations { get; set; } = new List<ServiceRequiredPart>();

    public virtual ICollection<ServiceStaffAssignment> ServiceStaffAssignmentCreatedUserNavigations { get; set; } = new List<ServiceStaffAssignment>();

    public virtual ICollection<ServiceStaffAssignment> ServiceStaffAssignmentStaffs { get; set; } = new List<ServiceStaffAssignment>();

    public virtual ICollection<ServiceStaffAssignment> ServiceStaffAssignmentUpdatedUserNavigations { get; set; } = new List<ServiceStaffAssignment>();

    public virtual ICollection<Service> ServiceUpdatedUserNavigations { get; set; } = new List<Service>();

    public virtual ICollection<Supplier> SupplierCreatedUserNavigations { get; set; } = new List<Supplier>();

    public virtual ICollection<Supplier> SupplierUpdatedUserNavigations { get; set; } = new List<Supplier>();

    public virtual AppUser? UpdatedUserNavigation { get; set; }
}
