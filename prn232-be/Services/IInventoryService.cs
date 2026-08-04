using BusinessObjects.DTOs;
using BusinessObjects.Models;
using BusinessObjects.ViewModels;
using System.Collections.Generic;

namespace Services
{
    public interface IInventoryService
    {
        InventoryReceiptResponseDto CreateInventoryReceipt(InventoryReceiptCreateDto dto, int staffId);
        bool AdjustInventory(InventoryAdjustmentViewModel dto, int staffId);
        IEnumerable<InventoryTransaction> GetTransactionsByPartId(int partId);
    }
}

