using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class MaintenanceAppointmentDAO
    {
        private static MaintenanceAppointmentDAO instance = null;
        private static readonly object instanceLock = new object();

        private MaintenanceAppointmentDAO() { }

        public static MaintenanceAppointmentDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new MaintenanceAppointmentDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<MaintenanceAppointment> GetAllAppointments()
        {
            using var context = new CarShowroomContext();
            return context.MaintenanceAppointments
                .Include(a => a.Customer)
                .Include(a => a.CustomerCar).ThenInclude(cc => cc.Brand)
                .Include(a => a.AppointmentDetails).ThenInclude(d => d.Package).ThenInclude(p => p.PackageServices).ThenInclude(ps => ps.Service)
                .Include(a => a.AppointmentDetails).ThenInclude(d => d.Service)
                .Include(a => a.ConsumedParts).ThenInclude(cp => cp.Part)
                .ToList();
        }

        public IEnumerable<MaintenanceAppointment> GetAppointmentsByCustomerId(int customerId)
        {
            using var context = new CarShowroomContext();
            return context.MaintenanceAppointments
                .Include(a => a.Customer)
                .Include(a => a.CustomerCar).ThenInclude(cc => cc.Brand)
                .Include(a => a.AppointmentDetails).ThenInclude(d => d.Package).ThenInclude(p => p.PackageServices).ThenInclude(ps => ps.Service)
                .Include(a => a.AppointmentDetails).ThenInclude(d => d.Service)
                .Include(a => a.ConsumedParts).ThenInclude(cp => cp.Part)
                .Where(a => a.CustomerId == customerId)
                .ToList();
        }

        public MaintenanceAppointment GetAppointmentById(int appointmentId)
        {
            using var context = new CarShowroomContext();
            return context.MaintenanceAppointments
                .Include(a => a.Customer)
                .Include(a => a.CustomerCar).ThenInclude(cc => cc.Brand)
                .Include(a => a.AppointmentDetails).ThenInclude(d => d.Package).ThenInclude(p => p.PackageServices).ThenInclude(ps => ps.Service)
                .Include(a => a.AppointmentDetails).ThenInclude(d => d.Service)
                .Include(a => a.ConsumedParts).ThenInclude(cp => cp.Part)
                .SingleOrDefault(a => a.AppointmentId == appointmentId);
        }

        public void AddAppointment(MaintenanceAppointment appointment)
        {
            using var context = new CarShowroomContext();
            context.MaintenanceAppointments.Add(appointment);
            context.SaveChanges();
        }

        /// <summary>
        /// Tạo appointment kèm AppointmentDetails và các Parts mua kèm trong cùng một transaction
        /// </summary>
        public MaintenanceAppointment CreateAppointmentWithDetails(MaintenanceAppointment appointment, List<AppointmentDetail> details, List<AppointmentConsumedPart>? parts = null)
        {
            using var context = new CarShowroomContext();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                context.MaintenanceAppointments.Add(appointment);
                context.SaveChanges();

                foreach (var detail in details)
                {
                    detail.AppointmentId = appointment.AppointmentId;
                    context.AppointmentDetails.Add(detail);
                }
                
                if (parts != null && parts.Any())
                {
                    foreach (var part in parts)
                    {
                        part.AppointmentId = appointment.AppointmentId;
                        part.AppointmentDetailId = null; // These are unified cart parts
                        part.IsIncurred = false; // Not incurred, they are standard parts bought alongside
                        context.AppointmentConsumedParts.Add(part);
                    }
                }
                
                context.SaveChanges();

                transaction.Commit();
                return appointment;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateAppointment(MaintenanceAppointment appointment)
        {
            using var context = new CarShowroomContext();
            context.Entry(appointment).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void DeleteAppointment(int appointmentId)
        {
            using var context = new CarShowroomContext();
            var appointment = context.MaintenanceAppointments.SingleOrDefault(a => a.AppointmentId == appointmentId);
            if (appointment != null)
            {
                context.MaintenanceAppointments.Remove(appointment);
                context.SaveChanges();
            }
        }
    }
}
