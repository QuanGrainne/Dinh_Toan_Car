using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class PartOrderDAO
    {
        private static PartOrderDAO instance = null;
        private static readonly object instanceLock = new object();

        private PartOrderDAO() { }

        public static PartOrderDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new PartOrderDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<PartOrder> GetAllOrders()
        {
            using var context = new CarShowroomContext();
            return context.PartOrders
                .Include(o => o.Customer)
                .Include(o => o.PartOrderDetails)
                    .ThenInclude(d => d.Part)
                .ToList();
        }

        public PartOrder? GetOrderById(int orderId)
        {
            using var context = new CarShowroomContext();
            return context.PartOrders
                .Include(o => o.Customer)
                .Include(o => o.PartOrderDetails)
                    .ThenInclude(d => d.Part)
                .SingleOrDefault(o => o.OrderId == orderId);
        }

        public IEnumerable<PartOrder> GetOrdersByCustomerId(int customerId)
        {
            using var context = new CarShowroomContext();
            return context.PartOrders
                .Include(o => o.Customer)
                .Include(o => o.PartOrderDetails)
                    .ThenInclude(d => d.Part)
                .Where(o => o.CustomerId == customerId)
                .ToList();
        }

        public void AddOrder(PartOrder order)
        {
            using var context = new CarShowroomContext();
            context.PartOrders.Add(order);
            context.SaveChanges();
        }

        public void UpdateOrder(PartOrder order)
        {
            using var context = new CarShowroomContext();
            context.Entry(order).State = EntityState.Modified;
            context.SaveChanges();
        }
    }
}
