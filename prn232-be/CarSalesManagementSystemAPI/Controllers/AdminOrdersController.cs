using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using DataAccessObjects;
using Microsoft.AspNetCore.Authorization;
using Services;

namespace CarSalesManagementSystemAPI.Controllers
{
    public class AppointmentConsumedPartInput
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }

    public class CancelOrderInput
    {
        public string? Reason { get; set; }
    }

    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IPartOrderService _partOrderService;
        private readonly IMaintenanceAppointmentService _maintenanceService;

        public AdminOrdersController(IPartOrderService partOrderService, IMaintenanceAppointmentService maintenanceService)
        {
            _partOrderService = partOrderService;
            _maintenanceService = maintenanceService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<AdminOrderListItemDto>> GetOrders()
        {
            try
            {
                using var context = new CarShowroomContext();
                var list = new List<AdminOrderListItemDto>();

                // 1. Get Part Orders
                var partOrders = context.PartOrders
                    .Include(o => o.PartOrderDetails)
                    .ToList();

                foreach (var po in partOrders)
                {
                    string paymentStatus = "NoInvoice";
                    if (po.MasterInvoiceId.HasValue)
                    {
                        var masterInvoice = context.MasterInvoices.Find(po.MasterInvoiceId.Value);
                        if (masterInvoice != null)
                        {
                            paymentStatus = masterInvoice.PaymentStatus;
                        }
                        else
                        {
                            paymentStatus = "Unpaid";
                        }
                    }
                    else
                    {
                        paymentStatus = "NoInvoice";
                    }

                    list.Add(new AdminOrderListItemDto
                    {
                        OrderCode = $"#PT-{po.OrderId:D4}",
                        OrderType = "Part",
                        SourceId = po.OrderId,
                        CustomerName = po.CustomerName,
                        CustomerPhone = po.CustomerPhone,
                        CreatedAt = po.CreatedAt,
                        TotalAmount = po.TotalAmount,
                        ProcessingStatus = (po.Status == "Shipping" && po.ShippingAddress != null && po.ShippingAddress.Contains("[Received]")) ? "Delivered" : po.Status,
                        PaymentStatus = paymentStatus,
                        AppointmentDateTime = null,
                        DeliveryMethod = po.DeliveryMethod
                    });
                }

                // 2. Get Maintenance Appointments (exclude Pending)
                var appointments = context.MaintenanceAppointments
                    .Include(a => a.AppointmentDetails)
                    .Include(a => a.ConsumedParts)
                    .Where(a => a.Status != "Pending")
                    .ToList();

                foreach (var appt in appointments)
                {
                    decimal totalAmount = 0;
                    string paymentStatus = "NoInvoice";

                    if (appt.MasterInvoiceId.HasValue)
                    {
                        var masterInvoice = context.MasterInvoices.Find(appt.MasterInvoiceId.Value);
                        if (masterInvoice != null)
                        {
                            totalAmount = masterInvoice.TotalAmount;
                            paymentStatus = masterInvoice.PaymentStatus;
                        }
                    }
                    else
                    {
                        // Calculate estimated cost
                        decimal serviceTotal = appt.AppointmentDetails?.Sum(d => d.UnitPrice * d.Quantity) ?? 0;
                        decimal partsTotal = appt.ConsumedParts?.Sum(p => p.UnitPrice * p.Quantity) ?? 0;
                        totalAmount = serviceTotal + partsTotal;
                        paymentStatus = "NoInvoice";
                    }

                    // Combined Appointment DateTime
                    DateTime? apptDateTime = null;
                    try
                    {
                        apptDateTime = appt.AppointmentDate.ToDateTime(appt.AppointmentTime);
                    }
                    catch { }

                    list.Add(new AdminOrderListItemDto
                    {
                        OrderCode = $"#SV-{appt.AppointmentId:D4}",
                        OrderType = "Maintenance",
                        SourceId = appt.AppointmentId,
                        CustomerName = appt.CustomerName,
                        CustomerPhone = appt.CustomerPhone,
                        CreatedAt = appt.CreatedAt,
                        TotalAmount = totalAmount,
                        ProcessingStatus = appt.Status,
                        PaymentStatus = paymentStatus,
                        AppointmentDateTime = apptDateTime
                    });
                }

                return Ok(list.OrderByDescending(o => o.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi lấy danh sách đơn hàng: " + ex.Message });
            }
        }

        // ==========================================
        // MAINTENANCE APPOINTMENT ORDER ACTIONS
        // ==========================================

        [HttpPut("maintenance/{id}/confirm")]
        public IActionResult ConfirmMaintenance(int id)
        {
            try
            {
                _maintenanceService.UpdateAppointmentStatus(id, "Confirmed");
                return Ok(new { success = true, message = "Xác nhận lịch bảo dưỡng thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("maintenance/{id}/start")]
        public IActionResult StartMaintenance(int id)
        {
            try
            {
                _maintenanceService.UpdateAppointmentStatus(id, "InProgress");
                return Ok(new { success = true, message = "Đã tiếp nhận xe và bắt đầu bảo dưỡng." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("maintenance/{id}/consumed-parts")]
        public IActionResult AddConsumedPart(int id, [FromBody] AppointmentConsumedPartInput dto)
        {
            using var context = new CarShowroomContext();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var part = context.Parts.Find(dto.PartId);
                if (part == null) return NotFound(new { message = "Không tìm thấy phụ tùng." });

                if (part.Quantity < dto.Quantity)
                    return BadRequest(new { message = $"Số lượng tồn kho không đủ (Còn: {part.Quantity})" });

                // Deduct stock
                part.Quantity -= dto.Quantity;
                if (part.Quantity == 0 || part.Quantity < part.MinStockLevel)
                {
                    part.Status = "OutOfStock";
                }
                context.Entry(part).State = EntityState.Modified;

                // Create InventoryTransaction Record
                var trans = new InventoryTransaction
                {
                    PartId = dto.PartId,
                    TransactionType = "Export",
                    Quantity = -dto.Quantity,
                    ReferenceType = "Maintenance",
                    ReferenceId = id,
                    StaffId = 1,
                    Notes = $"Sử dụng cho dịch vụ bảo dưỡng #{id}",
                    TransactionDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                context.InventoryTransactions.Add(trans);

                // Add consumed part
                var consumed = new AppointmentConsumedPart
                {
                    AppointmentId = id,
                    PartId = dto.PartId,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice ?? part.Price,
                    IsIncurred = true,
                    ApprovedByCustomer = true,
                    CreatedAt = DateTime.Now
                };
                context.AppointmentConsumedParts.Add(consumed);

                context.SaveChanges();
                transaction.Commit();
                return Ok(new { success = true, message = "Thêm phụ tùng phát sinh thành công và đã trừ kho." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new { message = "Lỗi thêm phụ tùng: " + ex.Message });
            }
        }

        [HttpPut("maintenance/{id}/complete")]
        public IActionResult CompleteMaintenance(int id)
        {
            try
            {
                _maintenanceService.UpdateAppointmentStatus(id, "Completed");
                return Ok(new { success = true, message = "Hoàn thành dịch vụ bảo dưỡng và đã tạo hóa đơn." });
            }
            catch (Exception ex)
            {
                var fullMessage = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    fullMessage += " --> " + inner.Message;
                    inner = inner.InnerException;
                }
                return BadRequest(new { message = fullMessage });
            }
        }

        [HttpPut("maintenance/{id}/confirm-payment")]
        public IActionResult ConfirmPaymentMaintenance(int id)
        {
            try
            {
                _maintenanceService.UpdateAppointmentPaymentStatus(id, true);
                return Ok(new { success = true, message = "Xác nhận thanh toán dịch vụ bảo dưỡng thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("maintenance/{id}/cancel")]
        public IActionResult CancelMaintenance(int id, [FromBody] CancelOrderInput? req)
        {
            try
            {
                string reason = req?.Reason ?? "Hủy bởi quản trị viên";
                _maintenanceService.UpdateAppointmentStatus(id, "Cancelled", reason);
                return Ok(new { success = true, message = "Hủy lịch bảo dưỡng thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================
        // PART ORDER ACTIONS
        // ==========================================

        [HttpPut("part/{id}/confirm")]
        public IActionResult ConfirmPartOrder(int id)
        {
            try
            {
                var order = _partOrderService.GetOrderById(id);
                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

                order.Status = "Confirmed";
                _partOrderService.UpdateOrder(order);
                return Ok(new { success = true, message = "Xác nhận đơn hàng phụ tùng lẻ thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("part/{id}/shipping")]
        public IActionResult ShippingPartOrder(int id)
        {
            try
            {
                var order = _partOrderService.GetOrderById(id);
                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

                order.Status = "Shipping";
                _partOrderService.UpdateOrder(order);
                return Ok(new { success = true, message = "Chuyển đơn hàng sang trạng thái đang giao hàng." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("part/{id}/complete")]
        public IActionResult CompletePartOrder(int id)
        {
            try
            {
                var order = _partOrderService.GetOrderById(id);
                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

                order.Status = "Completed";
                if (order.ShippingAddress != null && order.ShippingAddress.Contains("[Received]"))
                {
                    order.ShippingAddress = order.ShippingAddress.Replace(" [Received]", "");
                }
                _partOrderService.UpdateOrder(order);
                return Ok(new { success = true, message = "Hoàn thành đơn hàng phụ tùng lẻ." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("part/{id}/confirm-payment")]
        public IActionResult ConfirmPaymentPartOrder(int id)
        {
            try
            {
                using var context = new CarShowroomContext();
                var order = context.PartOrders.Find(id);
                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

                if (order.MasterInvoiceId.HasValue)
                {
                    var invoice = context.MasterInvoices.Find(order.MasterInvoiceId.Value);
                    if (invoice != null)
                    {
                        invoice.PaymentStatus = "Paid";
                        invoice.PaidAt = DateTime.Now;
                        invoice.UpdatedAt = DateTime.Now;
                        context.SaveChanges();
                        return Ok(new { success = true, message = "Xác nhận thanh toán đơn phụ tùng thành công." });
                    }
                }

                return BadRequest(new { message = "Đơn hàng chưa được xuất hóa đơn (phải xác nhận đơn trước)." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("part/{id}/cancel")]
        public IActionResult CancelPartOrder(int id)
        {
            try
            {
                var order = _partOrderService.GetOrderById(id);
                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

                order.Status = "Cancelled";
                _partOrderService.UpdateOrder(order);
                return Ok(new { success = true, message = "Hủy đơn hàng phụ tùng lẻ thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
