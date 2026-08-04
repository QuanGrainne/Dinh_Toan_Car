using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IMaintenancePackageRepository
    {
        IEnumerable<MaintenancePackage> GetAllPackages();
        IEnumerable<MaintenancePackage> GetAvailablePackages();
        MaintenancePackage GetPackageById(int packageId);
        void AddPackage(MaintenancePackage package);
        void UpdatePackage(MaintenancePackage package);
        void UpdatePackageWithServices(MaintenancePackage package, List<int> serviceIds);
        void DeletePackage(int packageId);
    }
}
