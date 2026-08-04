using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class AppointmentConsumedPartRepository : IAppointmentConsumedPartRepository
    {
        public IEnumerable<AppointmentConsumedPart> GetByAppointmentId(int appointmentId) => AppointmentConsumedPartDAO.Instance.GetByAppointmentId(appointmentId);
        public AppointmentConsumedPart GetById(int consumedPartId) => AppointmentConsumedPartDAO.Instance.GetById(consumedPartId);
        public void AddConsumedPart(AppointmentConsumedPart part) => AppointmentConsumedPartDAO.Instance.AddConsumedPart(part);
        public void UpdateConsumedPart(AppointmentConsumedPart part) => AppointmentConsumedPartDAO.Instance.UpdateConsumedPart(part);
        public void DeleteConsumedPart(int consumedPartId) => AppointmentConsumedPartDAO.Instance.DeleteConsumedPart(consumedPartId);
    }
}
