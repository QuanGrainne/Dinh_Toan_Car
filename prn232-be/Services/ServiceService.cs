using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Repositories;

namespace Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repository;

        public ServiceService(IServiceRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<ServiceDTO> GetAllServices()
        {
            var services = _repository.GetAllServices();
            return services.Select(MapToDTO);
        }

        public IEnumerable<ServiceDTO> GetAvailableServices()
        {
            var services = _repository.GetAvailableServices();
            return services.Select(MapToDTO);
        }

        public ServiceDTO GetServiceById(int serviceId)
        {
            var service = _repository.GetServiceById(serviceId);
            if (service == null) return null;
            return MapToDTO(service);
        }

        public void AddService(ServiceDTO serviceDto)
        {
            var service = new Service
            {
                ServiceName = serviceDto.ServiceName,
                Description = serviceDto.Description,
                BasePrice = serviceDto.BasePrice,
                EstimatedDurationMinutes = serviceDto.EstimatedDurationMinutes,
                Status = serviceDto.Status ?? "Available",
                CreatedAt = DateTime.Now
            };
            _repository.AddService(service);
        }

        public void UpdateService(ServiceDTO serviceDto)
        {
            var service = _repository.GetServiceById(serviceDto.ServiceId);
            if (service != null)
            {
                service.ServiceName = serviceDto.ServiceName;
                service.Description = serviceDto.Description;
                service.BasePrice = serviceDto.BasePrice;
                service.EstimatedDurationMinutes = serviceDto.EstimatedDurationMinutes;
                service.Status = serviceDto.Status;
                service.UpdatedAt = DateTime.Now;
                _repository.UpdateService(service);
            }
        }

        public void DeleteService(int serviceId) => _repository.DeleteService(serviceId);

        private ServiceDTO MapToDTO(Service service)
        {
            return new ServiceDTO
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Description = service.Description,
                BasePrice = service.BasePrice,
                EstimatedDurationMinutes = service.EstimatedDurationMinutes,
                Status = service.Status,
                CreatedAt = service.CreatedAt
            };
        }
    }
}
