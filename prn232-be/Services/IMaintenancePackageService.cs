using System.Collections.Generic;
using BusinessObjects.Models;

namespace Services
{
    public interface IMaintenancePackageService
    {
        IEnumerable<BusinessObjects.DTOs.MaintenancePackageDTO> GetAllPackages();
        IEnumerable<BusinessObjects.DTOs.MaintenancePackageDTO> GetAvailablePackages();
        BusinessObjects.DTOs.MaintenancePackageDTO GetPackageById(int packageId);
        void AddPackage(BusinessObjects.DTOs.MaintenancePackageDTO packageDto);
        void UpdatePackage(BusinessObjects.DTOs.MaintenancePackageDTO packageDto);
        void DeletePackage(int packageId);
    }
}
