using System;
using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        public void AddInventoryTransaction(InventoryTransaction transaction) =>
            InventoryDAO.Instance.AddInventoryTransaction(transaction);

        public IEnumerable<InventoryTransaction> GetTransactionsByPartId(int partId) =>
            InventoryDAO.Instance.GetTransactionsByPartId(partId);

        public InventoryReceipt CreateReceipt(InventoryReceipt receipt, List<InventoryReceiptDetail> details, Dictionary<int, DateTime?> partExpirations) =>
            InventoryDAO.Instance.CreateReceipt(receipt, details, partExpirations);
    }
}
