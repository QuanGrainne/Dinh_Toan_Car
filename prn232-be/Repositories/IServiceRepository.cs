using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IServiceRepository
    {
        IEnumerable<Service> GetAllServices();
        IEnumerable<Service> GetAvailableServices();
        Service GetServiceById(int serviceId);
        void AddService(Service service);
        void UpdateService(Service service);
        void DeleteService(int serviceId);
    }
}
