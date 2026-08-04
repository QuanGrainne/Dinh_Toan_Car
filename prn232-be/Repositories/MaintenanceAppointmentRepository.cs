using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class MaintenanceAppointmentRepository : IMaintenanceAppointmentRepository
    {
        public IEnumerable<MaintenanceAppointment> GetAllAppointments() => MaintenanceAppointmentDAO.Instance.GetAllAppointments();
        public IEnumerable<MaintenanceAppointment> GetAppointmentsByCustomerId(int customerId) => MaintenanceAppointmentDAO.Instance.GetAppointmentsByCustomerId(customerId);
        public MaintenanceAppointment GetAppointmentById(int appointmentId) => MaintenanceAppointmentDAO.Instance.GetAppointmentById(appointmentId);
        public void AddAppointment(MaintenanceAppointment appointment) => MaintenanceAppointmentDAO.Instance.AddAppointment(appointment);
        public MaintenanceAppointment CreateAppointmentWithDetails(MaintenanceAppointment appointment, List<AppointmentDetail> details, List<AppointmentConsumedPart>? parts = null) => MaintenanceAppointmentDAO.Instance.CreateAppointmentWithDetails(appointment, details, parts);
        public void UpdateAppointment(MaintenanceAppointment appointment) => MaintenanceAppointmentDAO.Instance.UpdateAppointment(appointment);
        public void DeleteAppointment(int appointmentId) => MaintenanceAppointmentDAO.Instance.DeleteAppointment(appointmentId);
    }
}
