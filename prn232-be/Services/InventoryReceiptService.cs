using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessObjects.Models;
using BusinessObjects.ViewModels;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class InventoryReceiptService : IInventoryReceiptService
    {
        private readonly ILogger<InventoryReceiptService> _logger;

        public InventoryReceiptService(ILogger<InventoryReceiptService> logger)
        {
            _logger = logger;
        }

        public async Task<ServiceResult<int>> CreateReceiptAsync(
            InventoryReceiptCreateViewModel request,
            int currentAdminId,
            CancellationToken cancellationToken = default
        )
        {
            using var context = new CarShowroomContext();
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Validate Items
                if (request.Items == null || request.Items.Count == 0)
                {
                    return ServiceResult<int>.Fail("Phải có ít nhất một dòng trong phiếu nhập.");
                }

                // Auto-fallback SupplierId if 0 for existing part rows
                int defaultSupId = request.SupplierId ?? 0;
                if (defaultSupId <= 0)
                {
                    var firstSup = await context.Suppliers.FirstOrDefaultAsync(s => s.Status == "Active", cancellationToken);
                    if (firstSup != null) defaultSupId = firstSup.SupplierId;
                }

                foreach (var item in request.Items)
                {
                    if (!item.SupplierId.HasValue || item.SupplierId.Value <= 0)
                    {
                        item.SupplierId = defaultSupId;
                    }
                }

                // Group items by SupplierId
                var itemsBySupplier = request.Items.GroupBy(x => x.SupplierId).ToList();
                int firstReceiptId = 0;

                foreach (var group in itemsBySupplier)
                {
                    int supplierId = group.Key.GetValueOrDefault();
                    if (supplierId <= 0)
                    {
                        return ServiceResult<int>.Fail("Vui lòng chọn nhà cung cấp cho mỗi dòng.");
                    }

                    var supplier = await context.Suppliers.SingleOrDefaultAsync(s => s.SupplierId == supplierId, cancellationToken);
                    if (supplier == null)
                    {
                        return ServiceResult<int>.Fail($"Nhà cung cấp (ID: {supplierId}) không tồn tại.");
                    }
                    if (supplier.Status != "Active")
                    {
                        return ServiceResult<int>.Fail($"Nhà cung cấp '{supplier.SupplierName}' đang ngưng hoạt động.");
                    }

                    // Create InventoryReceipt for this supplier
                    var receipt = new InventoryReceipt
                    {
                        SupplierId = supplierId,
                        StaffId = currentAdminId,
                        TotalAmount = 0,
                        ReceiptDate = DateTime.Now,
                        Notes = request.Notes?.Trim(),
                        CreatedAt = DateTime.Now,
                        CreatedUser = currentAdminId
                    };

                    context.InventoryReceipts.Add(receipt);
                    await context.SaveChangesAsync(cancellationToken);

                    if (firstReceiptId == 0)
                    {
                        firstReceiptId = receipt.ReceiptId;
                    }

                    decimal calculatedTotalAmount = 0;
                    var partIdSet = new System.Collections.Generic.HashSet<int>();

                    foreach (var item in group)
                    {
                        // Basic validation on quantity and import price
                        if (item.Quantity <= 0)
                        {
                            throw new InvalidOperationException("Số lượng phải lớn hơn 0.");
                        }
                        if (item.ImportPrice < 0)
                        {
                            throw new InvalidOperationException("Giá nhập không được âm.");
                        }
                        if (item.ExpiredAt.HasValue && item.ExpiredAt.Value.Date < DateTime.Today)
                        {
                            throw new InvalidOperationException("Hạn sử dụng không được nhỏ hơn ngày hiện tại.");
                        }

                        Part part;

                        if (item.IsNewPart)
                        {
                            // Validate new part view model
                            if (item.NewPart == null)
                            {
                                throw new InvalidOperationException("Thông tin phụ tùng mới không được trống.");
                            }
                            if (string.IsNullOrWhiteSpace(item.NewPart.PartName))
                            {
                                throw new InvalidOperationException("Tên phụ tùng mới không được rỗng.");
                            }
                            if (string.IsNullOrWhiteSpace(item.NewPart.PartCode))
                            {
                                throw new InvalidOperationException("Mã phụ tùng mới không được rỗng.");
                            }
                            if (item.NewPart.Price < 0)
                            {
                                throw new InvalidOperationException("Giá bán ra của phụ tùng mới không được âm.");
                            }
                            if (item.NewPart.MinStockLevel < 0 || item.NewPart.MaxStockLevel <= 0 || item.NewPart.MinStockLevel > item.NewPart.MaxStockLevel)
                            {
                                throw new InvalidOperationException("Hạn mức tồn kho không hợp lệ (Min <= Max, Max > 0).");
                            }
                            if (string.IsNullOrWhiteSpace(item.NewPart.UnitOfMeasure))
                            {
                                throw new InvalidOperationException("Đơn vị tính không được rỗng.");
                            }

                            // Validate CategoryId
                            var categoryExists = await context.PartCategories.AnyAsync(c => c.CategoryId == item.NewPart.CategoryId, cancellationToken);
                            if (!categoryExists)
                            {
                                throw new InvalidOperationException("Danh mục không tồn tại.");
                            }

                            // Validate PartCode uniqueness
                            var normalizedCode = item.NewPart.PartCode.Trim().ToUpperInvariant();
                            var codeExists = await context.Parts.AnyAsync(p => p.PartCode == normalizedCode, cancellationToken);
                            if (codeExists)
                            {
                                throw new InvalidOperationException($"Mã phụ tùng {normalizedCode} đã tồn tại.");
                            }

                            part = new Part
                            {
                                CategoryId = item.NewPart.CategoryId,
                                PartName = item.NewPart.PartName.Trim(),
                                PartCode = normalizedCode,
                                Brand = item.NewPart.Brand?.Trim(),
                                Price = item.NewPart.Price,
                                Quantity = 0, // Starts with 0 as required
                                MinStockLevel = item.NewPart.MinStockLevel,
                                MaxStockLevel = item.NewPart.MaxStockLevel,
                                UnitOfMeasure = item.NewPart.UnitOfMeasure.Trim(),
                                WarehouseLocation = item.NewPart.WarehouseLocation?.Trim(),
                                WarrantyMonths = item.NewPart.WarrantyMonths,
                                Description = item.NewPart.Description?.Trim(),
                                ImageUrl = item.NewPart.ImageUrl?.Trim(),
                                ExpiredAt = item.ExpiredAt,
                                Status = "OutOfStock",
                                CreatedAt = DateTime.Now,
                                CreatedUser = currentAdminId
                            };

                            context.Parts.Add(part);
                            await context.SaveChangesAsync(cancellationToken); // to get PartId
                        }
                        else
                        {
                            // Existing part
                            if (!item.PartId.HasValue)
                            {
                                throw new InvalidOperationException("PartId không được trống đối với phụ tùng có sẵn.");
                            }
                            if (partIdSet.Contains(item.PartId.Value))
                            {
                                throw new InvalidOperationException("Không chọn cùng một phụ tùng nhiều lần trong cùng một nhà cung cấp.");
                            }
                            partIdSet.Add(item.PartId.Value);

                            part = await context.Parts.SingleOrDefaultAsync(p => p.PartId == item.PartId.Value, cancellationToken);
                            if (part == null)
                            {
                                throw new InvalidOperationException("Phụ tùng không tồn tại.");
                            }
                            if (part.Status == "Inactive")
                            {
                                throw new InvalidOperationException($"Phụ tùng {part.PartName} đang ngưng hoạt động.");
                            }

                            // Rule for ExpiredAt logic
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

                        // Create InventoryReceiptDetail
                        var detail = new InventoryReceiptDetail
                        {
                            ReceiptId = receipt.ReceiptId,
                            PartId = part.PartId,
                            Quantity = item.Quantity,
                            ImportPrice = item.ImportPrice,
                            CreatedAt = DateTime.Now,
                            CreatedUser = currentAdminId
                        };
                        context.InventoryReceiptDetails.Add(detail);

                        // Create InventoryTransaction
                        var transactionRecord = new InventoryTransaction
                        {
                            PartId = part.PartId,
                            TransactionType = "Import",
                            Quantity = item.Quantity,
                            ReferenceType = "SupplierReceipt",
                            ReferenceId = receipt.ReceiptId,
                            StaffId = currentAdminId,
                            Notes = $"Nhập kho theo phiếu #{receipt.ReceiptId}",
                            TransactionDate = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            CreatedUser = currentAdminId
                        };
                        context.InventoryTransactions.Add(transactionRecord);

                        // Update part stock
                        part.Quantity += item.Quantity;
                        part.Status = "Available";
                        part.UpdatedAt = DateTime.Now;
                        part.UpdatedUser = currentAdminId;

                        calculatedTotalAmount += item.Quantity * item.ImportPrice;
                    }

                    // Update receipt TotalAmount
                    receipt.TotalAmount = calculatedTotalAmount;
                    receipt.UpdatedAt = DateTime.Now;
                    receipt.UpdatedUser = currentAdminId;
                }

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ServiceResult<int>.Ok(firstReceiptId, "Tạo phiếu nhập thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Lỗi tạo phiếu nhập kho phụ tùng");
                return ServiceResult<int>.Fail("Không thể tạo phiếu nhập. Vui lòng kiểm tra dữ liệu và thử lại: " + ex.Message);
            }
        }
    }
}
