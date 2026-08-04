using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IMaintenanceAppointmentRepository
    {
        IEnumerable<MaintenanceAppointment> GetAllAppointments();
        IEnumerable<MaintenanceAppointment> GetAppointmentsByCustomerId(int customerId);
        MaintenanceAppointment GetAppointmentById(int appointmentId);
        void AddAppointment(MaintenanceAppointment appointment);
        MaintenanceAppointment CreateAppointmentWithDetails(MaintenanceAppointment appointment, List<AppointmentDetail> details, List<AppointmentConsumedPart>? parts = null);
        void UpdateAppointment(MaintenanceAppointment appointment);
        void DeleteAppointment(int appointmentId);
    }
}
