using System;
using System.Collections.Generic;
using BusinessObjects.ModelsTemp;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.ContextTemp;

public partial class CarShowroomDbContext : DbContext
{
    public CarShowroomDbContext()
    {
    }

    public CarShowroomDbContext(DbContextOptions<CarShowroomDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppRole> AppRoles { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<AppointmentConsumedPart> AppointmentConsumedParts { get; set; }

    public virtual DbSet<AppointmentDetail> AppointmentDetails { get; set; }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<CarBrand> CarBrands { get; set; }

    public virtual DbSet<CarInvoice> CarInvoices { get; set; }

    public virtual DbSet<CustomerCar> CustomerCars { get; set; }

    public virtual DbSet<InventoryReceipt> InventoryReceipts { get; set; }

    public virtual DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<MaintenanceAppointment> MaintenanceAppointments { get; set; }

    public virtual DbSet<MaintenancePackage> MaintenancePackages { get; set; }

    public virtual DbSet<MasterInvoice> MasterInvoices { get; set; }

    public virtual DbSet<PackageService> PackageServices { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<PartCategory> PartCategories { get; set; }

    public virtual DbSet<PartCompatibility> PartCompatibilities { get; set; }

    public virtual DbSet<PartInvoice> PartInvoices { get; set; }

    public virtual DbSet<PartOrder> PartOrders { get; set; }

    public virtual DbSet<PartOrderDetail> PartOrderDetails { get; set; }

    public virtual DbSet<PurchaseRequest> PurchaseRequests { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceExecutionLog> ServiceExecutionLogs { get; set; }

    public virtual DbSet<ServiceInvoice> ServiceInvoices { get; set; }

    public virtual DbSet<ServiceRequiredPart> ServiceRequiredParts { get; set; }

    public virtual DbSet<ServiceStaffAssignment> ServiceStaffAssignments { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=CarShowroomDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__AppRoles__8AFACE1AF17C51FB");

            entity.HasIndex(e => e.RoleName, "UQ__AppRoles__8A2B6160D3B0996D").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.AppRoleCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_AppRoles_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.AppRoleUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_AppRoles_UpdatedUser");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__AppUsers__1788CC4CDF5A97CC");

            entity.HasIndex(e => e.Email, "UQ__AppUsers__A9D10534758CC889").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CodeExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.VerificationCode).HasMaxLength(100);

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.InverseCreatedUserNavigation)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_AppUsers_CreatedUser");

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppUsers_AppRoles");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.InverseUpdatedUserNavigation)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_AppUsers_UpdatedUser");
        });

        modelBuilder.Entity<AppointmentConsumedPart>(entity =>
        {
            entity.HasKey(e => e.ConsumedPartId).HasName("PK__Appointm__052BDE74C60A6BCA");

            entity.HasIndex(e => e.AppointmentId, "IX_AppointmentConsumedParts_AppointmentId");

            entity.Property(e => e.ApprovedByCustomer).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.AppointmentDetail).WithMany(p => p.AppointmentConsumedParts)
                .HasForeignKey(d => d.AppointmentDetailId)
                .HasConstraintName("FK_AppointmentConsumedParts_Details");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentConsumedParts)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_AppointmentConsumedParts_Appointments");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.AppointmentConsumedPartCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_AppointmentConsumedParts_CreatedUser");

            entity.HasOne(d => d.Part).WithMany(p => p.AppointmentConsumedParts)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentConsumedParts_Parts");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.AppointmentConsumedPartUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_AppointmentConsumedParts_UpdatedUser");
        });

        modelBuilder.Entity<AppointmentDetail>(entity =>
        {
            entity.HasKey(e => e.AppointmentDetailId).HasName("PK__Appointm__B475AFF5DC55AEF6");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", false)
                .HasColumnType("decimal(29, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentDetails)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_AppointmentDetails_Appointments");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.AppointmentDetailCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_AppointmentDetails_CreatedUser");

            entity.HasOne(d => d.Package).WithMany(p => p.AppointmentDetails)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK_AppointmentDetails_Packages");

            entity.HasOne(d => d.Service).WithMany(p => p.AppointmentDetails)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK_AppointmentDetails_Services");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.AppointmentDetailUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_AppointmentDetails_UpdatedUser");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__Cars__68A0342EC89E25B4");

            entity.HasIndex(e => e.BrandId, "IX_Cars_BrandId");

            entity.Property(e => e.CarName).HasMaxLength(150);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FuelType).HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");
            entity.Property(e => e.Transmission).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Brand).WithMany(p => p.Cars)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cars_CarBrands");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.CarCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_Cars_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.CarUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_Cars_UpdatedUser");
        });

        modelBuilder.Entity<CarBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__CarBrand__DAD4F05E20E4768F");

            entity.Property(e => e.BrandName).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.CarBrandCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_CarBrands_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.CarBrandUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_CarBrands_UpdatedUser");
        });

        modelBuilder.Entity<CarInvoice>(entity =>
        {
            entity.HasKey(e => e.CarInvoiceId).HasName("PK__CarInvoi__EC70F43A8E132FF9");

            entity.HasIndex(e => e.MasterInvoiceId, "IX_CarInvoices_MasterInvoiceId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InsuranceFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PlateFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RegistrationFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("((([UnitPrice]+[RegistrationFee])+[PlateFee])+[InsuranceFee])", false)
                .HasColumnType("decimal(21, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Car).WithMany(p => p.CarInvoices)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CarInvoices_Cars");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.CarInvoiceCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_CarInvoices_CreatedUser");

            entity.HasOne(d => d.MasterInvoice).WithMany(p => p.CarInvoices)
                .HasForeignKey(d => d.MasterInvoiceId)
                .HasConstraintName("FK_CarInvoices_MasterInvoices");

            entity.HasOne(d => d.PurchaseRequest).WithMany(p => p.CarInvoices)
                .HasForeignKey(d => d.PurchaseRequestId)
                .HasConstraintName("FK_CarInvoices_PurchaseRequests");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.CarInvoiceUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_CarInvoices_UpdatedUser");
        });

        modelBuilder.Entity<CustomerCar>(entity =>
        {
            entity.HasKey(e => e.CustomerCarId).HasName("PK__Customer__7842FE7C334EBFE2");

            entity.HasIndex(e => e.CustomerId, "IX_CustomerCars_CustomerId");

            entity.HasIndex(e => e.LicensePlate, "UQ__Customer__026BC15C54FF74C8").IsUnique();

            entity.HasIndex(e => e.Vin, "UQ__Customer__C5DF234C15C57BA6").IsUnique();

            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.LicensePlate).HasMaxLength(30);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Vin)
                .HasMaxLength(50)
                .HasColumnName("VIN");

            entity.HasOne(d => d.Brand).WithMany(p => p.CustomerCars)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerCars_CarBrands");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.CustomerCarCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_CustomerCars_CreatedUser");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerCarCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerCars_AppUsers");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.CustomerCarUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_CustomerCars_UpdatedUser");
        });

        modelBuilder.Entity<InventoryReceipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__Inventor__CC08C420EA37D837");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.ReceiptDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.InventoryReceiptCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_InventoryReceipts_CreatedUser");

            entity.HasOne(d => d.Staff).WithMany(p => p.InventoryReceiptStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryReceipts_AppUsers");

            entity.HasOne(d => d.Supplier).WithMany(p => p.InventoryReceipts)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryReceipts_Suppliers");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.InventoryReceiptUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_InventoryReceipts_UpdatedUser");
        });

        modelBuilder.Entity<InventoryReceiptDetail>(entity =>
        {
            entity.HasKey(e => e.ReceiptDetailId).HasName("PK__Inventor__82FADEFB4047E337");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImportPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([Quantity]*[ImportPrice])", false)
                .HasColumnType("decimal(29, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.InventoryReceiptDetailCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_InventoryReceiptDetails_CreatedUser");

            entity.HasOne(d => d.Part).WithMany(p => p.InventoryReceiptDetails)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryReceiptDetails_Parts");

            entity.HasOne(d => d.Receipt).WithMany(p => p.InventoryReceiptDetails)
                .HasForeignKey(d => d.ReceiptId)
                .HasConstraintName("FK_InventoryReceiptDetails_Receipts");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.InventoryReceiptDetailUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_InventoryReceiptDetails_UpdatedUser");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Inventor__55433A6B891C10D7");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.InventoryTransactionCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_InventoryTransactions_CreatedUser");

            entity.HasOne(d => d.Part).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryTransactions_Parts");

            entity.HasOne(d => d.Staff).WithMany(p => p.InventoryTransactionStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryTransactions_AppUsers");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.InventoryTransactionUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_InventoryTransactions_UpdatedUser");
        });

        modelBuilder.Entity<MaintenanceAppointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Maintena__8ECDFCC260D20A61");

            entity.HasIndex(e => e.CustomerId, "IX_MaintenanceAppointments_CustomerId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.MaintenanceAppointmentCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_MaintenanceAppointments_CreatedUser");

            entity.HasOne(d => d.CustomerCar).WithMany(p => p.MaintenanceAppointments)
                .HasForeignKey(d => d.CustomerCarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaintenanceAppointments_CustomerCars");

            entity.HasOne(d => d.Customer).WithMany(p => p.MaintenanceAppointmentCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaintenanceAppointments_AppUsers");

            entity.HasOne(d => d.MasterInvoice).WithMany(p => p.MaintenanceAppointments)
                .HasForeignKey(d => d.MasterInvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_MaintenanceAppointments_MasterInvoices");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.MaintenanceAppointmentUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_MaintenanceAppointments_UpdatedUser");
        });

        modelBuilder.Entity<MaintenancePackage>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__Maintena__322035CC8C1410EE");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PackageName).HasMaxLength(150);
            entity.Property(e => e.PackagePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.MaintenancePackageCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_MaintenancePackages_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.MaintenancePackageUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_MaintenancePackages_UpdatedUser");
        });

        modelBuilder.Entity<MasterInvoice>(entity =>
        {
            entity.HasKey(e => e.MasterInvoiceId).HasName("PK__MasterIn__7CB0B3080009D012");

            entity.HasIndex(e => e.CustomerId, "IX_MasterInvoices_CustomerId");

            entity.HasIndex(e => e.InvoiceNumber, "IX_MasterInvoices_InvoiceNumber");

            entity.HasIndex(e => e.InvoiceNumber, "UQ__MasterIn__D776E9815A27DF2F").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DepositCaptchaCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DepositCaptchaUsedAt).HasColumnType("datetime");
            entity.Property(e => e.DepositExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.DepositPaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.FinalCaptchaCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FinalCaptchaUsedAt).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.PurchaseType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Buyout");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalSubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.MasterInvoiceCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_MasterInvoices_CreatedUser");

            entity.HasOne(d => d.Customer).WithMany(p => p.MasterInvoiceCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MasterInvoices_Customers");

            entity.HasOne(d => d.Staff).WithMany(p => p.MasterInvoiceStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_MasterInvoices_Staff");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.MasterInvoiceUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_MasterInvoices_UpdatedUser");
        });

        modelBuilder.Entity<PackageService>(entity =>
        {
            entity.HasKey(e => new { e.PackageId, e.ServiceId }).HasName("PK__PackageS__8E718ECC464F99FB");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.PackageServiceCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_PackageServices_CreatedUser");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageServices)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK_PackageServices_Packages");

            entity.HasOne(d => d.Service).WithMany(p => p.PackageServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageServices_Services");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.PackageServiceUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_PackageServices_UpdatedUser");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("PK__Parts__7C3F0D50F86C2B10");

            entity.HasIndex(e => e.CategoryId, "IX_Parts_CategoryId");

            entity.HasIndex(e => e.PartCode, "IX_Parts_PartCode");

            entity.HasIndex(e => e.PartCode, "UQ__Parts__6525D39D76CE10D4").IsUnique();

            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.MaxStockLevel).HasDefaultValue(100);
            entity.Property(e => e.MinStockLevel).HasDefaultValue(5);
            entity.Property(e => e.PartCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartName).HasMaxLength(150);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(20)
                .HasDefaultValue("Cái");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.WarehouseLocation).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Parts)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parts_PartCategories");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.PartCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_Parts_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.PartUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_Parts_UpdatedUser");
        });

        modelBuilder.Entity<PartCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__PartCate__19093A0B970F73CD");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.PartCategoryCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_PartCategories_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.PartCategoryUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_PartCategories_UpdatedUser");
        });

        modelBuilder.Entity<PartCompatibility>(entity =>
        {
            entity.HasKey(e => e.CompatibilityId).HasName("PK__PartComp__D56A70EB9218785E");

            entity.HasIndex(e => e.PartId, "IX_PartCompatibilities_PartId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModelName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Brand).WithMany(p => p.PartCompatibilities)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartCompatibilities_CarBrands");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.PartCompatibilityCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_PartCompatibilities_CreatedUser");

            entity.HasOne(d => d.Part).WithMany(p => p.PartCompatibilities)
                .HasForeignKey(d => d.PartId)
                .HasConstraintName("FK_PartCompatibilities_Parts");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.PartCompatibilityUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_PartCompatibilities_UpdatedUser");
        });

        modelBuilder.Entity<PartInvoice>(entity =>
        {
            entity.HasKey(e => e.PartInvoiceId).HasName("PK__PartInvo__D3CD29C66089F068");

            entity.HasIndex(e => e.MasterInvoiceId, "IX_PartInvoices_MasterInvoiceId");

            entity.HasIndex(e => e.PartOrderId, "UQ__PartInvo__DAC1DADCCA4BABB3").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ShippingFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount)
                .HasComputedColumnSql("(([SubTotal]+[ShippingFee])+[TaxAmount])", false)
                .HasColumnType("decimal(20, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.PartInvoiceCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_PartInvoices_CreatedUser");

            entity.HasOne(d => d.MasterInvoice).WithMany(p => p.PartInvoices)
                .HasForeignKey(d => d.MasterInvoiceId)
                .HasConstraintName("FK_PartInvoices_MasterInvoices");

            entity.HasOne(d => d.PartOrder).WithOne(p => p.PartInvoice)
                .HasForeignKey<PartInvoice>(d => d.PartOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartInvoices_PartOrders");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.PartInvoiceUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_PartInvoices_UpdatedUser");
        });

        modelBuilder.Entity<PartOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__PartOrde__C3905BCF90405595");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.DeliveryMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Pickup");
            entity.Property(e => e.ShippingAddress).HasMaxLength(255);
            entity.Property(e => e.ShippingFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.PartOrderCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_PartOrders_CreatedUser");

            entity.HasOne(d => d.Customer).WithMany(p => p.PartOrderCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartOrders_AppUsers");

            entity.HasOne(d => d.MasterInvoice).WithMany(p => p.PartOrders)
                .HasForeignKey(d => d.MasterInvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_PartOrders_MasterInvoices");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.PartOrderUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_PartOrders_UpdatedUser");
        });

        modelBuilder.Entity<PartOrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__PartOrde__D3B9D36C7E267911");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", false)
                .HasColumnType("decimal(29, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.PartOrderDetailCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_PartOrderDetails_CreatedUser");

            entity.HasOne(d => d.Order).WithMany(p => p.PartOrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_PartOrderDetails_PartOrders");

            entity.HasOne(d => d.Part).WithMany(p => p.PartOrderDetails)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartOrderDetails_Parts");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.PartOrderDetailUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_PartOrderDetails_UpdatedUser");
        });

        modelBuilder.Entity<PurchaseRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Purchase__33A8517A3D9D6115");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Car).WithMany(p => p.PurchaseRequests)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseRequests_Cars");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.PurchaseRequestCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_PurchaseRequests_CreatedUser");

            entity.HasOne(d => d.Customer).WithMany(p => p.PurchaseRequestCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseRequests_AppUsers");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.PurchaseRequestUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_PurchaseRequests_UpdatedUser");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB00A783436CD");

            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EstimatedDurationMinutes).HasDefaultValue(30);
            entity.Property(e => e.ServiceName).HasMaxLength(150);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.ServiceCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_Services_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.ServiceUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_Services_UpdatedUser");
        });

        modelBuilder.Entity<ServiceExecutionLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ServiceE__5E548648DA28C9EC");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LogStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Started");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.RecordedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.AppointmentDetail).WithMany(p => p.ServiceExecutionLogs)
                .HasForeignKey(d => d.AppointmentDetailId)
                .HasConstraintName("FK_ServiceExecutionLogs_Details");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.ServiceExecutionLogCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_ServiceExecutionLogs_CreatedUser");

            entity.HasOne(d => d.Staff).WithMany(p => p.ServiceExecutionLogStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceExecutionLogs_Staff");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.ServiceExecutionLogUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_ServiceExecutionLogs_UpdatedUser");
        });

        modelBuilder.Entity<ServiceInvoice>(entity =>
        {
            entity.HasKey(e => e.ServiceInvoiceId).HasName("PK__ServiceI__3D59F6C58C4D1484");

            entity.HasIndex(e => e.MasterInvoiceId, "IX_ServiceInvoices_MasterInvoiceId");

            entity.HasIndex(e => e.AppointmentId, "UQ__ServiceI__8ECDFCC3EB2B541B").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LaborDiscount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount)
                .HasComputedColumnSql("([SubTotal]-[LaborDiscount])", false)
                .HasColumnType("decimal(19, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithOne(p => p.ServiceInvoice)
                .HasForeignKey<ServiceInvoice>(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceInvoices_Appointments");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.ServiceInvoiceCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_ServiceInvoices_CreatedUser");

            entity.HasOne(d => d.MasterInvoice).WithMany(p => p.ServiceInvoices)
                .HasForeignKey(d => d.MasterInvoiceId)
                .HasConstraintName("FK_ServiceInvoices_MasterInvoices");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.ServiceInvoiceUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_ServiceInvoices_UpdatedUser");
        });

        modelBuilder.Entity<ServiceRequiredPart>(entity =>
        {
            entity.HasKey(e => new { e.ServiceId, e.PartId }).HasName("PK__ServiceR__D2D840DF45BBCA7F");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuantityRequired).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.ServiceRequiredPartCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_ServiceRequiredParts_CreatedUser");

            entity.HasOne(d => d.Part).WithMany(p => p.ServiceRequiredParts)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequiredParts_Parts");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceRequiredParts)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK_ServiceRequiredParts_Services");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.ServiceRequiredPartUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_ServiceRequiredParts_UpdatedUser");
        });

        modelBuilder.Entity<ServiceStaffAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__ServiceS__32499E77B1CDC264");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Assigned");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Appointment).WithMany(p => p.ServiceStaffAssignments)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_ServiceStaffAssignments_Appointments");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.ServiceStaffAssignmentCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_ServiceStaffAssignments_CreatedUser");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceStaffAssignments)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceStaffAssignments_Services");

            entity.HasOne(d => d.Staff).WithMany(p => p.ServiceStaffAssignmentStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceStaffAssignments_Staff");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.ServiceStaffAssignmentUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_ServiceStaffAssignments_UpdatedUser");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666B498DA3563");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
            entity.Property(e => e.SupplierName).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.SupplierCreatedUserNavigations)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_Suppliers_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.SupplierUpdatedUserNavigations)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_Suppliers_UpdatedUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
