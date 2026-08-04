using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class PartOrderRepository : IPartOrderRepository
    {
        public IEnumerable<PartOrder> GetAllOrders() => PartOrderDAO.Instance.GetAllOrders();

        public PartOrder? GetOrderById(int orderId) => PartOrderDAO.Instance.GetOrderById(orderId);

        public IEnumerable<PartOrder> GetOrdersByCustomerId(int customerId) => PartOrderDAO.Instance.GetOrdersByCustomerId(customerId);

        public void AddOrder(PartOrder order) => PartOrderDAO.Instance.AddOrder(order);

        public void UpdateOrder(PartOrder order) => PartOrderDAO.Instance.UpdateOrder(order);
    }
}
