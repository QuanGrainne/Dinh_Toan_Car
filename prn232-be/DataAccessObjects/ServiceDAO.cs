using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class ServiceDAO
    {
        private static ServiceDAO instance = null;
        private static readonly object instanceLock = new object();

        private ServiceDAO() { }

        public static ServiceDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new ServiceDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<Service> GetAllServices()
        {
            using var context = new CarShowroomContext();
            return context.Services
                .ToList();
        }

        public IEnumerable<Service> GetAvailableServices()
        {
            using var context = new CarShowroomContext();
            return context.Services
                .Where(s => s.Status == "Available")
                .ToList();
        }

        public Service GetServiceById(int serviceId)
        {
            using var context = new CarShowroomContext();
            return context.Services
                .SingleOrDefault(s => s.ServiceId == serviceId);
        }

        public void AddService(Service service)
        {
            using var context = new CarShowroomContext();
            context.Services.Add(service);
            context.SaveChanges();
        }

        public void UpdateService(Service service)
        {
            using var context = new CarShowroomContext();
            context.Entry(service).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void DeleteService(int serviceId)
        {
            using var context = new CarShowroomContext();
            var service = context.Services.SingleOrDefault(s => s.ServiceId == serviceId);
            if (service != null)
            {
                try
                {
                    context.Services.Remove(service);
                    context.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    throw new Exception("Dịch vụ này đang được sử dụng trong gói bảo dưỡng hoặc lịch hẹn nên không thể xóa.");
                }
            }
        }
    }
}
