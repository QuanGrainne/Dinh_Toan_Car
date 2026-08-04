using System;
using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IInventoryRepository
    {
        void AddInventoryTransaction(InventoryTransaction transaction);
        IEnumerable<InventoryTransaction> GetTransactionsByPartId(int partId);
        InventoryReceipt CreateReceipt(InventoryReceipt receipt, List<InventoryReceiptDetail> details, Dictionary<int, DateTime?> partExpirations);
    }
}
