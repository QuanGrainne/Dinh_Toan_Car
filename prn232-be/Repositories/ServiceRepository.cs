using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        public IEnumerable<Service> GetAllServices() => ServiceDAO.Instance.GetAllServices();
        public IEnumerable<Service> GetAvailableServices() => ServiceDAO.Instance.GetAvailableServices();
        public Service GetServiceById(int serviceId) => ServiceDAO.Instance.GetServiceById(serviceId);
        public void AddService(Service service) => ServiceDAO.Instance.AddService(service);
        public void UpdateService(Service service) => ServiceDAO.Instance.UpdateService(service);
        public void DeleteService(int serviceId) => ServiceDAO.Instance.DeleteService(serviceId);
    }
}
