using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class AppointmentConsumedPartDAO
    {
        private static AppointmentConsumedPartDAO instance = null;
        private static readonly object instanceLock = new object();

        private AppointmentConsumedPartDAO() { }

        public static AppointmentConsumedPartDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new AppointmentConsumedPartDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<AppointmentConsumedPart> GetByAppointmentId(int appointmentId)
        {
            using var context = new CarShowroomContext();
            return context.AppointmentConsumedParts
                .Include(cp => cp.Part)
                .Where(cp => cp.AppointmentId == appointmentId)
                .ToList();
        }

        public AppointmentConsumedPart GetById(int consumedPartId)
        {
            using var context = new CarShowroomContext();
            return context.AppointmentConsumedParts
                .Include(cp => cp.Part)
                .SingleOrDefault(cp => cp.ConsumedPartId == consumedPartId);
        }

        public void AddConsumedPart(AppointmentConsumedPart part)
        {
            using var context = new CarShowroomContext();
            context.AppointmentConsumedParts.Add(part);
            context.SaveChanges();
        }

        public void UpdateConsumedPart(AppointmentConsumedPart part)
        {
            using var context = new CarShowroomContext();
            context.Entry(part).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void DeleteConsumedPart(int consumedPartId)
        {
            using var context = new CarShowroomContext();
            var part = context.AppointmentConsumedParts.SingleOrDefault(cp => cp.ConsumedPartId == consumedPartId);
            if (part != null)
            {
                context.AppointmentConsumedParts.Remove(part);
                context.SaveChanges();
            }
        }
    }
}
