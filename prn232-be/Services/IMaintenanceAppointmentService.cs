using System.Collections.Generic;
using BusinessObjects.Models;

namespace Services
{
    public interface IMaintenanceAppointmentService
    {
        IEnumerable<BusinessObjects.DTOs.MaintenanceAppointmentDTO> GetAllAppointments();
        IEnumerable<BusinessObjects.DTOs.MaintenanceAppointmentDTO> GetAppointmentsByCustomerId(int customerId);
        BusinessObjects.DTOs.MaintenanceAppointmentDTO GetAppointmentById(int appointmentId);
        BusinessObjects.DTOs.MaintenanceAppointmentDTO CreateAppointment(int customerId, BusinessObjects.DTOs.CreateAppointmentDTO createDto);
        void UpdateAppointmentStatus(int appointmentId, string status, string? reason = null);
        void UpdateAppointmentPaymentStatus(int appointmentId, bool isPaid);
        void UpdateAppointmentExtraFee(int appointmentId, decimal fee);
        void DeleteAppointment(int appointmentId);
    }
}
