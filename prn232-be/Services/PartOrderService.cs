using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Common;
using BusinessObjects.Models;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services
{
    public class PartOrderService : IPartOrderService
    {
        private readonly IPartOrderRepository _orderRepository;

        public PartOrderService(IPartOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public IEnumerable<PartOrder> GetAllOrders() => _orderRepository.GetAllOrders();

        public PartOrder? GetOrderById(int orderId) => _orderRepository.GetOrderById(orderId);

        public IEnumerable<PartOrder> GetOrdersByCustomerId(int customerId) => _orderRepository.GetOrdersByCustomerId(customerId);

        public void AddOrder(PartOrder order)
        {
            // Normalize Delivery Method
            if (string.Equals(order.DeliveryMethod, "HomeDelivery", StringComparison.OrdinalIgnoreCase))
            {
                order.DeliveryMethod = DeliveryMethods.Shipping;
            }
            else if (string.IsNullOrEmpty(order.DeliveryMethod))
            {
                order.DeliveryMethod = DeliveryMethods.Pickup;
            }

            // Validate Delivery Method vs Payment Method constraints
            if (order.DeliveryMethod == DeliveryMethods.Pickup)
            {
                if (order.PaymentMethod == PaymentMethods.COD)
                {
                    throw new InvalidOperationException("Phương thức thanh toán COD không áp dụng cho đơn hàng nhận tại showroom.");
                }
            }
            else if (order.DeliveryMethod == DeliveryMethods.Shipping)
            {
                if (string.IsNullOrWhiteSpace(order.ShippingAddress))
                {
                    throw new InvalidOperationException("Địa chỉ giao hàng là bắt buộc khi chọn giao hàng tận nơi.");
                }
                if (order.PaymentMethod == PaymentMethods.CashAtShowroom)
                {
                    throw new InvalidOperationException("Phương thức thanh toán bằng tiền mặt tại showroom không áp dụng cho đơn hàng giao tận nơi.");
                }
            }

            order.ShippingFee = order.DeliveryMethod switch
            {
                DeliveryMethods.Shipping => 30000m,
                _ => 0m
            };

            using var context = new CarShowroomContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                decimal subtotal = 0;
                var details = order.PartOrderDetails.ToList();

                if (!details.Any())
                {
                    throw new InvalidOperationException("Đơn hàng phải có ít nhất một phụ tùng.");
                }

                foreach (var detail in details)
                {
                    var part = context.Parts.SingleOrDefault(p => p.PartId == detail.PartId);
                    if (part == null)
                    {
                        throw new InvalidOperationException($"Không tìm thấy phụ tùng với ID: {detail.PartId}");
                    }

                    if (part.Status == "Inactive")
                    {
                        throw new InvalidOperationException($"Phụ tùng '{part.PartName}' hiện không khả dụng.");
                    }

                    if (detail.Quantity <= 0)
                    {
                        throw new InvalidOperationException($"Số lượng đặt hàng cho phụ tùng '{part.PartName}' phải lớn hơn 0.");
                    }

                    if (part.Quantity < detail.Quantity)
                    {
                        throw new InvalidOperationException($"Số lượng tồn kho của phụ tùng '{part.PartName}' không đủ (Còn lại: {part.Quantity}).");
                    }

                    // Lock price server-side from database
                    detail.UnitPrice = part.Price;
                    detail.SubTotal = part.Price * detail.Quantity;
                    subtotal += detail.SubTotal;
                }

                order.TotalAmount = subtotal + order.ShippingFee;
                order.CreatedAt = DateTime.Now;
                order.Status = PartOrderStatuses.Pending; // Stock is NOT deducted in Pending status

                context.PartOrders.Add(order);
                context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new InvalidOperationException(ExceptionMessageHelper.GetDetailedMessage(ex), ex);
            }
        }

        public void UpdateOrder(PartOrder order)
        {
            using var context = new CarShowroomContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                var dbOrder = context.PartOrders
                    .Include(o => o.PartOrderDetails)
                        .ThenInclude(d => d.Part)
                    .SingleOrDefault(o => o.OrderId == order.OrderId);

                if (dbOrder == null)
                {
                    throw new InvalidOperationException("Không tìm thấy đơn hàng cần cập nhật.");
                }

                string oldStatus = dbOrder.Status;
                string newStatus = order.Status;

                // Nếu đơn đã được gộp vào một hóa đơn tổng (qua /api/checkout) thì hóa đơn + trừ kho
                // đã được xử lý ở đó — chỉ cập nhật trạng thái, không tạo master/trừ kho lần nữa.
                if (dbOrder.MasterInvoiceId.HasValue &&
                    newStatus == PartOrderStatuses.Confirmed && oldStatus == PartOrderStatuses.Pending)
                {
                    dbOrder.Status = PartOrderStatuses.Confirmed;
                }
                // Handle Admin Confirmation (Pending -> Confirmed)
                else if (newStatus == PartOrderStatuses.Confirmed && oldStatus == PartOrderStatuses.Pending)
                {
                    decimal subtotal = 0;
                    var pendingTransactions = new List<InventoryTransaction>();

                    // Verify stock and deduct inventory
                    foreach (var detail in dbOrder.PartOrderDetails)
                    {
                        var part = context.Parts.SingleOrDefault(p => p.PartId == detail.PartId);
                        if (part == null)
                        {
                            throw new InvalidOperationException($"Không tìm thấy phụ tùng ID #{detail.PartId}");
                        }

                        if (part.Quantity < detail.Quantity)
                        {
                            throw new InvalidOperationException($"Số lượng tồn kho cho '{part.PartName}' không đủ để xác nhận đơn hàng.");
                        }

                        // Deduct stock
                        part.Quantity -= detail.Quantity;
                        if (part.Quantity == 0 || part.Quantity < part.MinStockLevel)
                        {
                            part.Status = "OutOfStock";
                        }
                        context.Entry(part).State = EntityState.Modified;

                        subtotal += detail.SubTotal;

                        // Create InventoryTransaction Export record
                        var invTx = new InventoryTransaction
                        {
                            PartId = part.PartId,
                            TransactionType = InventoryTransactionTypes.Export,
                            Quantity = -detail.Quantity, // Negative for export
                            ReferenceType = InventoryReferenceTypes.PartOrder,
                            ReferenceId = dbOrder.OrderId,
                            StaffId = dbOrder.CustomerId,
                            Notes = $"Xuất kho xác nhận đơn phụ tùng #{dbOrder.OrderId}",
                            TransactionDate = DateTime.Now,
                            CreatedAt = DateTime.Now,
                            CreatedUser = dbOrder.CustomerId
                        };
                        pendingTransactions.Add(invTx);
                    }

                    // Create MasterInvoice for Part Order
                    var masterInvoice = new MasterInvoice
                    {
                        InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{dbOrder.OrderId:D4}",
                        InvoiceType = InvoiceTypes.Part,
                        CustomerId = dbOrder.CustomerId,
                        TotalSubTotal = subtotal,
                        DiscountAmount = 0,
                        TaxAmount = 0,
                        TotalAmount = dbOrder.TotalAmount,
                        PaymentStatus = PaymentStatuses.Unpaid,
                        InvoiceStatus = InvoiceStatuses.Confirmed,
                        PaymentMethod = dbOrder.PaymentMethod,
                        PurchaseType = "Buyout",
                        CreatedAt = DateTime.Now
                    };
                    context.MasterInvoices.Add(masterInvoice);
                    context.SaveChanges(); // Generate MasterInvoiceId

                    // Create PartInvoice linking MasterInvoice to PartOrder
                    var partInvoice = new PartInvoice
                    {
                        MasterInvoiceId = masterInvoice.MasterInvoiceId,
                        PartOrderId = dbOrder.OrderId,
                        SubTotal = subtotal,
                        ShippingFee = dbOrder.ShippingFee,
                        TaxAmount = 0,
                        TotalAmount = dbOrder.TotalAmount,
                        CreatedAt = DateTime.Now
                    };
                    context.PartInvoices.Add(partInvoice);

                    foreach (var tx in pendingTransactions)
                    {
                        context.InventoryTransactions.Add(tx);
                    }

                    dbOrder.MasterInvoiceId = masterInvoice.MasterInvoiceId;
                    dbOrder.Status = PartOrderStatuses.Confirmed;
                }
                // Handle Cancellation (Pending or Confirmed -> Cancelled)
                else if (newStatus == PartOrderStatuses.Cancelled && oldStatus != PartOrderStatuses.Cancelled)
                {
                    // Restore stock ONLY IF stock was previously deducted (Confirmed or Shipping)
                    if (oldStatus == PartOrderStatuses.Confirmed || oldStatus == PartOrderStatuses.Shipping)
                    {
                        foreach (var detail in dbOrder.PartOrderDetails)
                        {
                            var part = context.Parts.SingleOrDefault(p => p.PartId == detail.PartId);
                            if (part != null)
                            {
                                part.Quantity += detail.Quantity;
                                if ((part.Status == "OutOfStock" || part.Status == "Out of Stock") && part.Quantity > 0)
                                {
                                    part.Status = "Available";
                                }

                                context.Entry(part).State = EntityState.Modified;

                                // Log Return InventoryTransaction
                                var invTx = new InventoryTransaction
                                {
                                    PartId = part.PartId,
                                    TransactionType = InventoryTransactionTypes.Return,
                                    Quantity = detail.Quantity, // Positive for return
                                    ReferenceType = InventoryReferenceTypes.PartOrder,
                                    ReferenceId = dbOrder.OrderId,
                                    StaffId = dbOrder.CustomerId,
                                    Notes = $"Hoàn kho do hủy đơn hàng phụ tùng #{dbOrder.OrderId}",
                                    TransactionDate = DateTime.Now,
                                    CreatedAt = DateTime.Now,
                                    CreatedUser = dbOrder.CustomerId
                                };
                                context.InventoryTransactions.Add(invTx);
                            }
                        }
                    }

                    dbOrder.Status = PartOrderStatuses.Cancelled;

                    if (dbOrder.MasterInvoiceId.HasValue)
                    {
                        var masterInvoice = context.MasterInvoices.Find(dbOrder.MasterInvoiceId.Value);
                        if (masterInvoice != null)
                        {
                            masterInvoice.InvoiceStatus = InvoiceStatuses.Cancelled;
                            masterInvoice.UpdatedAt = DateTime.Now;
                        }
                    }
                }
                // Handle Completion (Confirmed / Shipping -> Completed)
                else if (newStatus == PartOrderStatuses.Completed)
                {
                    dbOrder.Status = PartOrderStatuses.Completed;

                    if (dbOrder.MasterInvoiceId.HasValue)
                    {
                        var masterInvoice = context.MasterInvoices.Find(dbOrder.MasterInvoiceId.Value);
                        if (masterInvoice != null)
                        {
                            masterInvoice.PaymentStatus = PaymentStatuses.Paid;
                            masterInvoice.InvoiceStatus = InvoiceStatuses.Completed;
                            masterInvoice.PaidAt = DateTime.Now;
                            masterInvoice.UpdatedAt = DateTime.Now;
                        }
                    }
                }
                else
                {
                    dbOrder.Status = newStatus;
                }

                dbOrder.UpdatedAt = DateTime.Now;
                if (order.CustomerName != null) dbOrder.CustomerName = order.CustomerName;
                if (order.CustomerPhone != null) dbOrder.CustomerPhone = order.CustomerPhone;
                if (order.CustomerEmail != null) dbOrder.CustomerEmail = order.CustomerEmail;
                if (order.ShippingAddress != null) dbOrder.ShippingAddress = order.ShippingAddress;
                if (order.DeliveryMethod != null) dbOrder.DeliveryMethod = order.DeliveryMethod;

                context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new InvalidOperationException(ExceptionMessageHelper.GetDetailedMessage(ex), ex);
            }
        }
    }
}
