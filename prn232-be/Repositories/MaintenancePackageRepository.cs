using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class MaintenancePackageRepository : IMaintenancePackageRepository
    {
        public IEnumerable<MaintenancePackage> GetAllPackages() => MaintenancePackageDAO.Instance.GetAllPackages();
        public IEnumerable<MaintenancePackage> GetAvailablePackages() => MaintenancePackageDAO.Instance.GetAvailablePackages();
        public MaintenancePackage GetPackageById(int packageId) => MaintenancePackageDAO.Instance.GetPackageById(packageId);
        public void AddPackage(MaintenancePackage package) => MaintenancePackageDAO.Instance.AddPackage(package);
        public void UpdatePackage(MaintenancePackage package) => MaintenancePackageDAO.Instance.UpdatePackage(package);
        public void UpdatePackageWithServices(MaintenancePackage package, List<int> serviceIds) => MaintenancePackageDAO.Instance.UpdatePackageWithServices(package, serviceIds);
        public void DeletePackage(int packageId) => MaintenancePackageDAO.Instance.DeletePackage(packageId);
    }
}
