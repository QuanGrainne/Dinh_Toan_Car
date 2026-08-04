using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IAppointmentConsumedPartRepository
    {
        IEnumerable<AppointmentConsumedPart> GetByAppointmentId(int appointmentId);
        AppointmentConsumedPart GetById(int consumedPartId);
        void AddConsumedPart(AppointmentConsumedPart part);
        void UpdateConsumedPart(AppointmentConsumedPart part);
        void DeleteConsumedPart(int consumedPartId);
    }
}
