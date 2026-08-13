using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using BusinessObjects.Models;

namespace DataAccessObjects;

public partial class CarShowroomContext : DbContext
{
    public CarShowroomContext()
    {
    }

    public CarShowroomContext(DbContextOptions<CarShowroomContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppRole> AppRoles { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<CarBrand> CarBrands { get; set; }

    public virtual DbSet<MaintenancePackage> MaintenancePackages { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<PackageService> PackageServices { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<PartCategory> PartCategories { get; set; }

    public override int SaveChanges()
    {
        TextEncodingNormalizer.NormalizePendingStrings(this);
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        TextEncodingNormalizer.NormalizePendingStrings(this);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TextEncodingNormalizer.NormalizePendingStrings(this);
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        TextEncodingNormalizer.NormalizePendingStrings(this);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__AppRoles__8AFACE1ABED980DF");

            entity.HasIndex(e => e.RoleName, "UQ__AppRoles__8A2B6160E1219BDE").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);

            entity.HasData(
                new AppRole { RoleId = 1, RoleName = "Admin" },
                new AppRole { RoleId = 2, RoleName = "Customer" }
            );
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__AppUsers__1788CC4CD3D16938");

            entity.HasIndex(e => e.Email, "UQ__AppUsers__A9D10534CD643903").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppUsers_AppRoles");

            entity.HasData(
                new AppUser
                {
                    UserId = 1,
                    FullName = "System Admin",
                    Email = "admin@gmail.com",
                    PasswordHash = "$2a$11$ivuFcskipHfVJyUk7X7Cy.72DYWJAKQhFt7uaF2kMrwZ/LAHW1cWO", // password: admin
                    PhoneNumber = "0987654321",
                    Address = "Hanoi",
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new AppUser
                {
                    UserId = 2,
                    FullName = "John Customer",
                    Email = "customer@gmail.com",
                    PasswordHash = "$2a$11$iR0JU.l1mLeRCyKuClJFxuWqtweaw2kS3oZSRG/lAcD00M603P5Mm", // password: customer
                    PhoneNumber = "0123456789",
                    Address = "HCM City",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1)
                }
            );
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__Cars__68A0342E9C46E9E9");

            entity.Property(e => e.CarName).HasMaxLength(150);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FuelType).HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.AdditionalImages);
            entity.Property(e => e.ReviewUrl).HasMaxLength(500);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");
            entity.Property(e => e.Transmission).HasMaxLength(50);

            entity.HasOne(d => d.Brand).WithMany(p => p.Cars)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cars_CarBrands");

            entity.HasData(
                new Car
                {
                    CarId = 1,
                    BrandId = 1,
                    CarName = "Toyota Camry 2.5Q",
                    Model = "Camry",
                    Year = 2022,
                    Color = "Black",
                    Mileage = 15000,
                    FuelType = "Gasoline",
                    Transmission = "Automatic",
                    Price = 1350000000,
                    Description = "Xe sang tr?ng, l?ch l?m, gia d?nh s? d?ng k?, b?o du?ng ch?nh h?ng.",
                    ImageUrl = "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Car
                {
                    CarId = 2,
                    BrandId = 1,
                    CarName = "Toyota Vios 1.5G",
                    Model = "Vios",
                    Year = 2021,
                    Color = "White",
                    Mileage = 28000,
                    FuelType = "Gasoline",
                    Transmission = "Automatic",
                    Price = 520000000,
                    Description = "Xe qu?c d?n ti?t ki?m nhi?n li?u, v?n h?nh b?n b?.",
                    ImageUrl = "https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Car
                {
                    CarId = 3,
                    BrandId = 2,
                    CarName = "Ford Ranger Wildtrak 2.0L",
                    Model = "Ranger",
                    Year = 2023,
                    Color = "Orange",
                    Mileage = 8000,
                    FuelType = "Diesel",
                    Transmission = "Automatic",
                    Price = 960000000,
                    Description = "Vua b?n t?i, phi?n b?n cao c?p nh?t Wildtrak 2 c?u, d?y d? c?ng ngh?.",
                    ImageUrl = "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Car
                {
                    CarId = 4,
                    BrandId = 3,
                    CarName = "VinFast VF8 Plus",
                    Model = "VF8",
                    Year = 2023,
                    Color = "Blue",
                    Mileage = 5000,
                    FuelType = "Electric",
                    Transmission = "Automatic",
                    Price = 1100000000,
                    Description = "Xe di?n th?ng minh Vi?t Nam, b?n Plus pin SDI, c?ng ngh? ADAS hi?n d?i.",
                    ImageUrl = "https://images.unsplash.com/photo-1563720223185-11003d516935?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Car
                {
                    CarId = 5,
                    BrandId = 4,
                    CarName = "BMW 320i Sport Line",
                    Model = "3 Series",
                    Year = 2020,
                    Color = "Red",
                    Mileage = 35000,
                    FuelType = "Gasoline",
                    Transmission = "Automatic",
                    Price = 1250000000,
                    Description = "D?ng sedan th? thao l?i c?c hay, ngo?i h?nh tr? trung nang d?ng.",
                    ImageUrl = "https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Car
                {
                    CarId = 6,
                    BrandId = 3,
                    CarName = "VinFast VF5 Plus",
                    Model = "VF5",
                    Year = 2023,
                    Color = "Gray",
                    Mileage = 2000,
                    FuelType = "Electric",
                    Transmission = "Automatic",
                    Price = 450000000,
                    Description = "Xe d? th? c? nh? th?ng minh, c?c k? ti?t ki?m v? nh? g?n.",
                    ImageUrl = "https://images.unsplash.com/photo-1617788138017-80ad40651399?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                }
            );
        });

        modelBuilder.Entity<CarBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__CarBrand__DAD4F05EFE11BDE9");

            entity.Property(e => e.BrandName).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasData(
                new CarBrand { BrandId = 1, BrandName = "Toyota", Country = "Japan", Description = "Toyota Motor Corporation" },
                new CarBrand { BrandId = 2, BrandName = "Ford", Country = "USA", Description = "Ford Motor Company" },
                new CarBrand { BrandId = 3, BrandName = "VinFast", Country = "Vietnam", Description = "VinFast Vietnam" },
                new CarBrand { BrandId = 4, BrandName = "BMW", Country = "Germany", Description = "Bayerische Motoren Werke AG" }
            );
        });


        modelBuilder.Entity<MaintenancePackage>(entity =>
        {
            entity.HasKey(e => e.PackageId);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PackageName).HasMaxLength(150);
            entity.Property(e => e.PackagePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");

            entity.HasData(
                new MaintenancePackage
                {
                    PackageId = 1,
                    PackageName = "Bao duong Dinh ky Tieu chuan 10.000km",
                    Description = "Goi bao duong co ban giup xe van hanh tron tru bao gom thay dau, kiem tra phanh va ra soat loi.",
                    PackagePrice = 1200000,
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new MaintenancePackage
                {
                    PackageId = 2,
                    PackageName = "Cham soc Dieu hoa VIP don he",
                    Description = "Lam lanh sau, diet khuan dan lanh dieu hoa noi that.",
                    PackagePrice = 950000,
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                }
            );
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId);

            entity.Property(e => e.ServiceName).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EstimatedDurationMinutes).HasDefaultValue(30);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasData(
                new Service { ServiceId = 1, ServiceName = "Thay dau dong co & Coc loc dau", Description = "Xa dau cu, thay loc dau chinh hang, cham dau dong co Castrol moi phu hop.", BasePrice = 200000, EstimatedDurationMinutes = 30, Status = "Available", CreatedAt = new DateTime(2025, 1, 1) },
                new Service { ServiceId = 2, ServiceName = "Can chinh thuoc lai do chum lop", Description = "Su dung may quet laser 3D de can chinh do chum banh xe va can bang dong.", BasePrice = 450000, EstimatedDurationMinutes = 45, Status = "Available", CreatedAt = new DateTime(2025, 1, 1) },
                new Service { ServiceId = 3, ServiceName = "Ve sinh dan lanh dieu hoa noi that", Description = "Su dung may noi soi chuyen dung lam sach bui ban dan lanh khong can thao taplo.", BasePrice = 600000, EstimatedDurationMinutes = 60, Status = "Available", CreatedAt = new DateTime(2025, 1, 1) },
                new Service { ServiceId = 4, ServiceName = "Kiem tra toan dien 30 hang muc ky thuat", Description = "Kiem tra may gam, phanh, lop, dien than xe, nuoc lam mat, chan doan loi bang may chuyen dung.", BasePrice = 150000, EstimatedDurationMinutes = 40, Status = "Available", CreatedAt = new DateTime(2025, 1, 1) }
            );
        });

        modelBuilder.Entity<PackageService>(entity =>
        {
            entity.HasKey(e => new { e.PackageId, e.ServiceId });

            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageServices)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PackageServices_Packages");

            entity.HasOne(d => d.Service).WithMany(p => p.PackageServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageServices_Services");

            entity.HasData(
                new PackageService { PackageId = 1, ServiceId = 1, CreatedAt = new DateTime(2025, 1, 1) },
                new PackageService { PackageId = 1, ServiceId = 4, CreatedAt = new DateTime(2025, 1, 1) },
                new PackageService { PackageId = 2, ServiceId = 3, CreatedAt = new DateTime(2025, 1, 1) },
                new PackageService { PackageId = 2, ServiceId = 4, CreatedAt = new DateTime(2025, 1, 1) }
            );
        });



        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("PK__Parts__7C3F0D509A440590");

            entity.HasIndex(e => e.PartCode, "UQ__Parts__6525D39D6EAC6A52").IsUnique();

            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.PartCode).HasMaxLength(50);
            entity.Property(e => e.PartName).HasMaxLength(150);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Category).WithMany(p => p.Parts)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parts_PartCategories");

            entity.HasOne(d => d.CreatedUserNavigation).WithMany(p => p.CreatedParts)
                .HasForeignKey(d => d.CreatedUser)
                .HasConstraintName("FK_Parts_CreatedUser");

            entity.HasOne(d => d.UpdatedUserNavigation).WithMany(p => p.UpdatedParts)
                .HasForeignKey(d => d.UpdatedUser)
                .HasConstraintName("FK_Parts_UpdatedUser");


            entity.HasData(
                new Part
                {
                    PartId = 1,
                    CategoryId = 4,
                    PartName = "L?p xe Michelin Pilot Sport 4",
                    PartCode = "PT-MIC-PS4",
                    Brand = "Michelin",
                    Price = 3200000,
                    Quantity = 40,
                    Description = "L?p hi?u nang cao, b?m du?ng c?c t?t trong m?i di?u ki?n th?i ti?t.",
                    ImageUrl = "https://images.unsplash.com/photo-1578844251758-2f71da64c96f?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Part
                {
                    PartId = 2,
                    CategoryId = 2,
                    PartName = "?c quy GS 12V 45Ah",
                    PartCode = "PT-GS-12V45",
                    Brand = "GS Battery",
                    Price = 1450000,
                    Quantity = 25,
                    Description = "?c quy kh? mi?n b?o du?ng, d? b?n cao, kh?i d?ng m?nh m?.",
                    ImageUrl = "https://images.unsplash.com/photo-1619642751034-765dfdf7c58e?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Part
                {
                    PartId = 3,
                    CategoryId = 3,
                    PartName = "D?u nh?t Castrol Magnatec 5W-30",
                    PartCode = "PT-CAS-5W30",
                    Brand = "Castrol",
                    Price = 850000,
                    Quantity = 50,
                    Description = "D?u nh?t c?ng ngh? t?ng h?p ho?n to?n b?o v? d?ng co ngay khi kh?i d?ng.",
                    ImageUrl = "https://images.unsplash.com/photo-1622560480605-d83c853bc5c3?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Part
                {
                    PartId = 4,
                    CategoryId = 4,
                    PartName = "G?t mua Bosch Aerotwin",
                    PartCode = "PT-BOS-AERO",
                    Brand = "Bosch",
                    Price = 450000,
                    Quantity = 60,
                    Description = "G?t mua cao c?p t? Bosch D?c, g?t s?ch nu?c nh? nh?ng, ?m ?i.",
                    ImageUrl = "https://images.unsplash.com/photo-1517524206127-48bbd363f3d7?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                },
                new Part
                {
                    PartId = 5,
                    CategoryId = 2,
                    PartName = "D?n pha LED Philips Ultinon Essential",
                    PartCode = "PT-PHI-LEDH7",
                    Brand = "Philips",
                    Price = 1200000,
                    Quantity = 15,
                    Description = "B?ng d?n LED H7 si?u s?ng, gom s?ng t?t, d? b?n l?n d?n 5 nam.",
                    ImageUrl = "https://images.unsplash.com/photo-1508974239320-0a029497e820?auto=format&fit=crop&w=600&q=80",
                    Status = "Available",
                    CreatedAt = new DateTime(2025, 1, 1)
                }
            );
        });

        modelBuilder.Entity<PartCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__PartCate__19093A0B38EB3018");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasData(
                new PartCategory { CategoryId = 1, CategoryName = "D?ng co & Truy?n d?ng", Description = "C?c b? ph?n li?n quan d?n d?ng co, h?p s? v? truy?n d?ng." },
                new PartCategory { CategoryId = 2, CategoryName = "H? th?ng di?n & ?c quy", Description = "?c quy, m?y ph?t di?n, d?n v? h? th?ng di?n." },
                new PartCategory { CategoryId = 3, CategoryName = "D?u nh?t & H?a ch?t", Description = "D?u m?y, nu?c l?m m?t, d?u phanh v? h?a ch?t b?o du?ng." },
                new PartCategory { CategoryId = 4, CategoryName = "Ngo?i th?t & Ph? ki?n", Description = "L?p xe, g?t mua, guong v? c?c ph? ki?n trang tr? ngo?i th?t." }
            );
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
