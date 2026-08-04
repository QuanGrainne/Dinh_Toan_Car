using System.Collections.Generic;
using BusinessObjects.DTOs;

namespace Services
{
    public interface IServiceService
    {
        IEnumerable<ServiceDTO> GetAllServices();
        IEnumerable<ServiceDTO> GetAvailableServices();
        ServiceDTO GetServiceById(int serviceId);
        void AddService(ServiceDTO serviceDto);
        void UpdateService(ServiceDTO serviceDto);
        void DeleteService(int serviceId);
    }
}
