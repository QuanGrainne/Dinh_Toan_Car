using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;

namespace DataAccessObjects
{
    public partial class CarShowroomContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // Configure Part extensions
            modelBuilder.Entity<Part>(entity =>
            {
                entity.Property(e => e.MinStockLevel).HasDefaultValue(5);
                entity.Property(e => e.MaxStockLevel).HasDefaultValue(100);
                entity.Property(e => e.UnitOfMeasure).HasMaxLength(20).HasDefaultValue("Cái");
                entity.Property(e => e.WarehouseLocation).HasMaxLength(100);
                entity.Property(e => e.WarrantyMonths).HasDefaultValue(0);
                entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedUserNavigation)
                    .WithMany(p => p.CreatedParts)
                    .HasForeignKey(d => d.CreatedUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Parts_CreatedUser");

                entity.HasOne(d => d.UpdatedUserNavigation)
                    .WithMany(p => p.UpdatedParts)
                    .HasForeignKey(d => d.UpdatedUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Parts_UpdatedUser");
            });

            // Configure PartCategory extensions
            modelBuilder.Entity<PartCategory>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedUserNavigation)
                    .WithMany(p => p.CreatedPartCategories)
                    .HasForeignKey(d => d.CreatedUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartCategories_CreatedUser");

                entity.HasOne(d => d.UpdatedUserNavigation)
                    .WithMany(p => p.UpdatedPartCategories)
                    .HasForeignKey(d => d.UpdatedUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartCategories_UpdatedUser");
            });
        }
    }
}
