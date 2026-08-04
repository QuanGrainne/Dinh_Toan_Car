using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class InventoryDAO
    {
        private static InventoryDAO? instance = null;
        private static readonly object instanceLock = new object();

        private InventoryDAO() { }

        public static InventoryDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new InventoryDAO();
                    }
                    return instance;
                }
            }
        }

        public void AddInventoryTransaction(InventoryTransaction transaction)
        {
            using var context = new CarShowroomContext();
            context.InventoryTransactions.Add(transaction);
            context.SaveChanges();
        }

        public IEnumerable<InventoryTransaction> GetTransactionsByPartId(int partId)
        {
            using var context = new CarShowroomContext();
            return context.InventoryTransactions
                .Include(t => t.Staff)
                .Where(t => t.PartId == partId)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
        }

        public InventoryReceipt CreateReceipt(InventoryReceipt receipt, List<InventoryReceiptDetail> details, Dictionary<int, DateTime?> partExpirations)
        {
            using var context = new CarShowroomContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                // Verify supplier
                var supplier = context.Suppliers.SingleOrDefault(s => s.SupplierId == receipt.SupplierId);
                if (supplier == null)
                {
                    throw new InvalidOperationException("Nhà cung cấp không tồn tại.");
                }

                // Add receipt
                receipt.CreatedAt = DateTime.Now;
                receipt.ReceiptDate = DateTime.Now;
                context.InventoryReceipts.Add(receipt);
                context.SaveChanges(); // Saves receipt to generate ReceiptId

                decimal totalAmount = 0;

                foreach (var detail in details)
                {
                    var part = context.Parts.SingleOrDefault(p => p.PartId == detail.PartId);
                    if (part == null)
                    {
                        throw new InvalidOperationException($"Phụ tùng với ID '{detail.PartId}' không tồn tại.");
                    }

                    // Check max stock level constraint
                    if (part.Quantity + detail.Quantity > part.MaxStockLevel)
                    {
                        throw new InvalidOperationException($"Không thể nhập hàng: Số lượng nhập ({detail.Quantity}) cộng số lượng hiện tại ({part.Quantity}) vượt quá sức chứa tối đa của kho ({part.MaxStockLevel}) cho phụ tùng '{part.PartName}'.");
                    }

                    // Update quantity
                    part.Quantity += detail.Quantity;
                    
                    // Update ExpiredAt if provided for this part
                    if (partExpirations.TryGetValue(part.PartId, out var expDate))
                    {
                        part.ExpiredAt = expDate;
                    }

                    // Update status
                    if (part.Quantity > 0 && (part.Status == "OutOfStock" || part.Status == "Out of Stock"))
                    {
                        part.Status = "Available";
                    }

                    context.Entry(part).State = EntityState.Modified;

                    // Save receipt detail
                    detail.ReceiptId = receipt.ReceiptId;
                    detail.CreatedAt = DateTime.Now;
                    context.InventoryReceiptDetails.Add(detail);
                    
                    totalAmount += detail.Quantity * detail.ImportPrice;

                    // Log inventory transaction (Audit Trail)
                    var invTx = new InventoryTransaction
                    {
                        PartId = part.PartId,
                        TransactionType = "Import",
                        Quantity = detail.Quantity,
                        ReferenceType = "SupplierReceipt",
                        ReferenceId = receipt.ReceiptId,
                        StaffId = receipt.StaffId,
                        Notes = $"Nhập hàng từ NCC: {supplier.SupplierName}. " + receipt.Notes,
                        TransactionDate = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        CreatedUser = receipt.StaffId
                    };
                    context.InventoryTransactions.Add(invTx);
                }

                // Update total amount on receipt
                receipt.TotalAmount = totalAmount;
                context.Entry(receipt).State = EntityState.Modified;

                context.SaveChanges();
                transaction.Commit();

                return receipt;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
