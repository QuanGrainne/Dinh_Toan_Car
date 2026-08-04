using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IPartOrderRepository
    {
        IEnumerable<PartOrder> GetAllOrders();
        PartOrder? GetOrderById(int orderId);
        IEnumerable<PartOrder> GetOrdersByCustomerId(int customerId);
        void AddOrder(PartOrder order);
        void UpdateOrder(PartOrder order);
    }
}
