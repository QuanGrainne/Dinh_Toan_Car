using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Repositories;

namespace Services
{
    public class MaintenancePackageService : IMaintenancePackageService
    {
        private readonly IMaintenancePackageRepository _repository;

        public MaintenancePackageService(IMaintenancePackageRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<MaintenancePackageDTO> GetAllPackages() 
        {
            var packages = _repository.GetAllPackages();
            return packages.Select(MapToDTO);
        }

        public IEnumerable<MaintenancePackageDTO> GetAvailablePackages() 
        {
            var packages = _repository.GetAvailablePackages();
            return packages.Select(MapToDTO);
        }

        public MaintenancePackageDTO GetPackageById(int packageId) 
        {
            var package = _repository.GetPackageById(packageId);
            if (package == null) return null;
            return MapToDTO(package);
        }

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Available";
            if (status.Equals("Unavailable", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return "Inactive";
            }
            return "Available";
        }

        public void AddPackage(MaintenancePackageDTO packageDto) 
        {
            var package = new MaintenancePackage
            {
                PackageName = packageDto.PackageName,
                Description = packageDto.Description,
                PackagePrice = packageDto.PackagePrice,
                Status = NormalizeStatus(packageDto.Status),
                CreatedAt = DateTime.Now
            };

            if (packageDto.ServiceIds != null && packageDto.ServiceIds.Any())
            {
                foreach (var serviceId in packageDto.ServiceIds)
                {
                    package.PackageServices.Add(new PackageService
                    {
                        ServiceId = serviceId,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            _repository.AddPackage(package);
        }

        public void UpdatePackage(MaintenancePackageDTO packageDto) 
        {
            var package = new MaintenancePackage
            {
                PackageId = packageDto.PackageId,
                PackageName = packageDto.PackageName,
                Description = packageDto.Description,
                PackagePrice = packageDto.PackagePrice,
                Status = NormalizeStatus(packageDto.Status),
                UpdatedAt = DateTime.Now
            };

            if (packageDto.ServiceIds != null)
            {
                _repository.UpdatePackageWithServices(package, packageDto.ServiceIds);
            }
            else
            {
                _repository.UpdatePackage(package);
            }
        }

        public void DeletePackage(int packageId) => _repository.DeletePackage(packageId);

        private MaintenancePackageDTO MapToDTO(MaintenancePackage package)
        {
            return new MaintenancePackageDTO
            {
                PackageId = package.PackageId,
                PackageName = package.PackageName,
                Description = package.Description,
                PackagePrice = package.PackagePrice,
                Status = package.Status,
                CreatedAt = package.CreatedAt,
                Services = package.PackageServices.Select(ps => new ServiceSummaryDTO
                {
                    ServiceId = ps.Service.ServiceId,
                    ServiceName = ps.Service.ServiceName,
                    BasePrice = ps.Service.BasePrice,
                    EstimatedDurationMinutes = ps.Service.EstimatedDurationMinutes
                }).ToList(),
                ServiceIds = package.PackageServices.Select(ps => ps.ServiceId).ToList()
            };
        }
    }
}
