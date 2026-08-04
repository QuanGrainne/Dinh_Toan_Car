using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Repositories;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IPartRepository _partRepository;
        private readonly IPartCategoryRepository _categoryRepository;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IPartRepository partRepository,
            IPartCategoryRepository categoryRepository)
        {
            _inventoryRepository = inventoryRepository;
            _partRepository = partRepository;
            _categoryRepository = categoryRepository;
        }

        public InventoryReceiptResponseDto CreateInventoryReceipt(InventoryReceiptCreateDto dto, int staffId)
        {
            using var context = new CarShowroomContext();
            using var dbTransaction = context.Database.BeginTransaction();

            try
            {
                // Step 1: Validate general inputs
                if (dto.SupplierId <= 0)
                {
                    throw new InvalidOperationException("Nhà cung cấp không hợp lệ.");
                }
                var supplier = context.Suppliers.SingleOrDefault(s => s.SupplierId == dto.SupplierId);
                if (supplier == null)
                {
                    throw new InvalidOperationException("Nhà cung cấp không tồn tại.");
                }

                if (dto.Items == null || dto.Items.Count == 0)
                {
                    throw new InvalidOperationException("Danh sách phụ tùng nhập kho không được để trống.");
                }

                // Check staffId validity
                var staff = context.AppUsers.SingleOrDefault(u => u.UserId == staffId);
                if (staff == null)
                {
                    throw new InvalidOperationException("Nhân viên thực hiện không tồn tại.");
                }

                // Check duplicate PartIds in the request
                var existingPartIds = new HashSet<int>();
                foreach (var item in dto.Items)
                {
                    if (!item.IsNewPart)
                    {
                        if (!item.PartId.HasValue)
                        {
                            throw new InvalidOperationException("Mã ID phụ tùng trống.");
                        }
                        if (existingPartIds.Contains(item.PartId.Value))
                        {
                            var duplicatePart = context.Parts.Find(item.PartId.Value);
                            string partName = duplicatePart?.PartName ?? $"ID {item.PartId.Value}";
                            throw new InvalidOperationException($"Phụ tùng '{partName}' đã có trong phiếu nhập. Vui lòng cập nhật số lượng ở dòng hiện tại.");
                        }
                        existingPartIds.Add(item.PartId.Value);
                    }
                }

                // Validate each item DTO parameters
                foreach (var item in dto.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        throw new InvalidOperationException("Số lượng nhập kho phải lớn hơn 0.");
                    }
                    if (item.ImportPrice < 0)
                    {
                        throw new InvalidOperationException("Giá nhập kho không được âm.");
                    }
                    if (item.ExpiredAt.HasValue && item.ExpiredAt.Value.Date < DateTime.Today)
                    {
                        throw new InvalidOperationException("Hạn sử dụng không được nhỏ hơn ngày hiện tại.");
                    }

                    if (item.IsNewPart)
                    {
                        if (item.NewPart == null)
                        {
                            throw new InvalidOperationException("Thông tin phụ tùng mới không được để trống.");
                        }
                        if (item.NewPart.CategoryId <= 0)
                        {
                            throw new InvalidOperationException("Danh mục phụ tùng mới là bắt buộc.");
                        }
                        var categoryExists = context.PartCategories.Any(c => c.CategoryId == item.NewPart.CategoryId);
                        if (!categoryExists)
                        {
                            throw new InvalidOperationException($"Danh mục với ID {item.NewPart.CategoryId} không tồn tại.");
                        }
                        if (string.IsNullOrWhiteSpace(item.NewPart.PartName))
                        {
                            throw new InvalidOperationException("Tên phụ tùng mới không được để trống.");
                        }
                        if (string.IsNullOrWhiteSpace(item.NewPart.PartCode))
                        {
                            throw new InvalidOperationException("Mã phụ tùng mới không được để trống.");
                        }
                        var normalizedCode = item.NewPart.PartCode.Trim().ToUpperInvariant();
                        var codeExists = context.Parts.Any(p => p.PartCode.Trim().ToUpper() == normalizedCode);
                        if (codeExists)
                        {
                            throw new InvalidOperationException($"Mã phụ tùng {normalizedCode} đã tồn tại.");
                        }
                        if (item.NewPart.Price < 0)
                        {
                            throw new InvalidOperationException("Giá bán ra không được âm.");
                        }
                        if (item.NewPart.MinStockLevel < 0)
                        {
                            throw new InvalidOperationException("Định mức tối thiểu không được âm.");
                        }
                        if (item.NewPart.MaxStockLevel <= 0)
                        {
                            throw new InvalidOperationException("Định mức tối đa phải lớn hơn 0.");
                        }
                        if (item.NewPart.MinStockLevel > item.NewPart.MaxStockLevel)
                        {
                            throw new InvalidOperationException("Định mức tối thiểu không được lớn hơn định mức tối đa.");
                        }
                        if (item.NewPart.WarrantyMonths < 0)
                        {
                            throw new InvalidOperationException("Số tháng bảo hành không được âm.");
                        }
                    }
                    else
                    {
                        var part = context.Parts.SingleOrDefault(p => p.PartId == (item.PartId ?? 0));
                        if (part == null)
                        {
                            throw new InvalidOperationException($"Phụ tùng với ID '{item.PartId}' không tồn tại.");
                        }
                        if (part.Status == "Inactive")
                        {
                            throw new InvalidOperationException($"Không thể nhập kho phụ tùng '{part.PartName}' vì đang ở trạng thái ngưng hoạt động (Inactive).");
                        }
                    }
                }

                // Step 2: Create inventory receipt
                var receipt = new InventoryReceipt
                {
                    SupplierId = dto.SupplierId,
                    StaffId = staffId,
                    ReceiptDate = DateTime.Now,
                    Notes = dto.Notes,
                    TotalAmount = 0,
                    CreatedAt = DateTime.Now,
                    CreatedUser = staffId
                };
                context.InventoryReceipts.Add(receipt);
                context.SaveChanges(); // Get ReceiptId

                decimal totalAmount = 0;

                // Step 3: Process items
                foreach (var item in dto.Items)
                {
                    Part part;
                    if (item.IsNewPart)
                    {
                        var normalizedCode = item.NewPart!.PartCode.Trim().ToUpperInvariant();
                        part = new Part
                        {
                            CategoryId = item.NewPart.CategoryId,
                            PartName = item.NewPart.PartName.Trim(),
                            PartCode = normalizedCode,
                            Brand = item.NewPart.Brand?.Trim(),
                            Price = item.NewPart.Price,
                            Quantity = 0,
                            MinStockLevel = item.NewPart.MinStockLevel,
                            MaxStockLevel = item.NewPart.MaxStockLevel,
                            UnitOfMeasure = string.IsNullOrWhiteSpace(item.NewPart.UnitOfMeasure) ? "Cái" : item.NewPart.UnitOfMeasure.Trim(),
                            WarehouseLocation = item.NewPart.WarehouseLocation?.Trim(),
                            WarrantyMonths = item.NewPart.WarrantyMonths,
                            Description = item.NewPart.Description?.Trim(),
                            ImageUrl = item.NewPart.ImageUrl?.Trim(),
                            ExpiredAt = item.ExpiredAt,
                            Status = "OutOfStock",
                            CreatedAt = DateTime.Now,
                            CreatedUser = staffId
                        };
                        context.Parts.Add(part);
                        context.SaveChanges(); // Get PartId
                    }
                    else
                    {
                        part = context.Parts.Single(p => p.PartId == (item.PartId ?? 0));

                        // Expiration date tracking logic
                        if (item.ExpiredAt.HasValue)
                        {
                            if (!part.ExpiredAt.HasValue || part.Quantity == 0)
                            {
                                part.ExpiredAt = item.ExpiredAt;
                            }
                            else if (item.ExpiredAt.Value < part.ExpiredAt.Value)
                            {
                                part.ExpiredAt = item.ExpiredAt;
                            }
                        }
                    }

                    // Step 4: Create receipt details
                    var detail = new InventoryReceiptDetail
                    {
                        ReceiptId = receipt.ReceiptId,
                        PartId = part.PartId,
                        Quantity = item.Quantity,
                        ImportPrice = item.ImportPrice,
                        CreatedAt = DateTime.Now,
                        CreatedUser = staffId
                    };
                    context.InventoryReceiptDetails.Add(detail);

                    // Step 5: Log transaction
                    var invTx = new InventoryTransaction
                    {
                        PartId = part.PartId,
                        TransactionType = "Import",
                        Quantity = item.Quantity,
                        ReferenceType = "SupplierReceipt",
                        ReferenceId = receipt.ReceiptId,
                        StaffId = staffId,
                        Notes = $"Nhập kho theo phiếu #{receipt.ReceiptId}. Ghi chú: {receipt.Notes}",
                        TransactionDate = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        CreatedUser = staffId
                    };
                    context.InventoryTransactions.Add(invTx);

                    // Step 6: Update inventory quantity & status
                    part.Quantity += item.Quantity;
                    part.Status = "Available";
                    part.UpdatedAt = DateTime.Now;
                    part.UpdatedUser = staffId;
                    context.Entry(part).State = EntityState.Modified;

                    totalAmount += item.Quantity * item.ImportPrice;
                }

                // Step 7: Update total amount
                receipt.TotalAmount = totalAmount;
                context.Entry(receipt).State = EntityState.Modified;
                
                context.SaveChanges();
                dbTransaction.Commit();

                string message = $"Đã lưu phiếu nhập kho #{receipt.ReceiptId} thành công với tổng số tiền {totalAmount.ToString("N0")} đ.";
                return new InventoryReceiptResponseDto
                {
                    ReceiptId = receipt.ReceiptId,
                    TotalAmount = totalAmount,
                    Status = "Success",
                    Message = message
                };
            }
            catch (Exception)
            {
                dbTransaction.Rollback();
                throw;
            }
        }

        public bool AdjustInventory(BusinessObjects.ViewModels.InventoryAdjustmentViewModel dto, int staffId)
        {
            if (dto.PartId <= 0)
            {
                throw new InvalidOperationException("Phụ tùng không hợp lệ.");
            }
            if (dto.AdjustmentQuantity == 0)
            {
                throw new InvalidOperationException("Số lượng điều chỉnh phải khác 0.");
            }
            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                throw new InvalidOperationException("Lý do điều chỉnh không được để trống.");
            }

            using var context = new CarShowroomContext();
            using var dbTransaction = context.Database.BeginTransaction();

            try
            {
                var part = context.Parts.SingleOrDefault(p => p.PartId == dto.PartId);
                if (part == null)
                {
                    throw new InvalidOperationException("Không tìm thấy phụ tùng cần điều chỉnh.");
                }

                int newQuantity = part.Quantity + dto.AdjustmentQuantity;
                if (newQuantity < 0)
                {
                    throw new InvalidOperationException($"Số lượng tồn kho sau điều chỉnh không được âm (Hiện tại: {part.Quantity}, Điều chỉnh: {dto.AdjustmentQuantity}).");
                }

                string notesText = string.IsNullOrWhiteSpace(dto.Notes)
                    ? dto.Reason.Trim()
                    : $"{dto.Reason.Trim()} - {dto.Notes.Trim()}";

                var transactionRecord = new InventoryTransaction
                {
                    PartId = dto.PartId,
                    TransactionType = "Adjustment",
                    Quantity = dto.AdjustmentQuantity,
                    ReferenceType = "Adjustment",
                    ReferenceId = null,
                    StaffId = staffId,
                    Notes = notesText,
                    TransactionDate = DateTime.Now
                };
                context.InventoryTransactions.Add(transactionRecord);

                part.Quantity = newQuantity;
                part.UpdatedAt = DateTime.Now;
                part.UpdatedUser = staffId;

                if (part.Quantity == 0 && part.Status != "Inactive")
                {
                    part.Status = "OutOfStock";
                }
                else if (part.Quantity > 0 && part.Status == "OutOfStock")
                {
                    part.Status = "Available";
                }

                context.SaveChanges();
                dbTransaction.Commit();
                return true;
            }
            catch (Exception)
            {
                dbTransaction.Rollback();
                throw;
            }
        }

        public IEnumerable<InventoryTransaction> GetTransactionsByPartId(int partId)
        {
            return _inventoryRepository.GetTransactionsByPartId(partId);
        }
    }
}

