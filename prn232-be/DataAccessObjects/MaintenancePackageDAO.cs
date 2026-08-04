using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class MaintenancePackageDAO
    {
        private static MaintenancePackageDAO instance = null;
        private static readonly object instanceLock = new object();

        private MaintenancePackageDAO() { }

        public static MaintenancePackageDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new MaintenancePackageDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<MaintenancePackage> GetAllPackages()
        {
            using var context = new CarShowroomContext();
            return context.MaintenancePackages
                .Include(p => p.PackageServices).ThenInclude(ps => ps.Service)
                .ToList();
        }

        public IEnumerable<MaintenancePackage> GetAvailablePackages()
        {
            using var context = new CarShowroomContext();
            return context.MaintenancePackages
                .Include(p => p.PackageServices).ThenInclude(ps => ps.Service)
                .Where(p => p.Status == "Available")
                .ToList();
        }

        public MaintenancePackage GetPackageById(int packageId)
        {
            using var context = new CarShowroomContext();
            return context.MaintenancePackages
                .Include(p => p.PackageServices).ThenInclude(ps => ps.Service)
                .SingleOrDefault(p => p.PackageId == packageId);
        }

        public void AddPackage(MaintenancePackage package)
        {
            using var context = new CarShowroomContext();
            context.MaintenancePackages.Add(package);
            context.SaveChanges();
        }

        /// <summary>
        /// Cập nhật package kèm danh sách services (thay thế toàn bộ PackageServices)
        /// </summary>
        public void UpdatePackageWithServices(MaintenancePackage package, List<int> serviceIds)
        {
            using var context = new CarShowroomContext();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var existingPackage = context.MaintenancePackages
                    .Include(p => p.PackageServices)
                    .FirstOrDefault(p => p.PackageId == package.PackageId);

                if (existingPackage != null)
                {
                    existingPackage.PackageName = package.PackageName;
                    existingPackage.Description = package.Description;
                    existingPackage.PackagePrice = package.PackagePrice;
                    existingPackage.Status = package.Status ?? "Available";
                    existingPackage.UpdatedAt = package.UpdatedAt ?? DateTime.Now;

                    // Xóa tất cả PackageServices cũ
                    context.PackageServices.RemoveRange(existingPackage.PackageServices);

                    // Thêm PackageServices mới
                    if (serviceIds != null)
                    {
                        foreach (var serviceId in serviceIds)
                        {
                            context.PackageServices.Add(new PackageService
                            {
                                PackageId = existingPackage.PackageId,
                                ServiceId = serviceId,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }

                    context.SaveChanges();
                    transaction.Commit();
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdatePackage(MaintenancePackage package)
        {
            using var context = new CarShowroomContext();
            var existingPackage = context.MaintenancePackages.FirstOrDefault(p => p.PackageId == package.PackageId);
            if (existingPackage != null)
            {
                existingPackage.PackageName = package.PackageName;
                existingPackage.Description = package.Description;
                existingPackage.PackagePrice = package.PackagePrice;
                existingPackage.Status = package.Status ?? "Available";
                existingPackage.UpdatedAt = package.UpdatedAt ?? DateTime.Now;
                context.SaveChanges();
            }
        }

        public void DeletePackage(int packageId)
        {
            using var context = new CarShowroomContext();
            var package = context.MaintenancePackages.SingleOrDefault(p => p.PackageId == packageId);
            if (package != null)
            {
                try
                {
                    context.MaintenancePackages.Remove(package);
                    context.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    throw new Exception("Không thể xóa gói bảo dưỡng này vì đã có khách hàng đặt lịch.");
                }
            }
        }
    }
}
