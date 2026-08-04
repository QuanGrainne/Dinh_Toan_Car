using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Services;
using DataAccessObjects;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/customer/orders")]
    [ApiController]
    [Authorize]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly IMaintenanceAppointmentService _appointmentService;
        private readonly IPartOrderService _partOrderService;

        public CustomerOrdersController(
            IMaintenanceAppointmentService appointmentService,
            IPartOrderService partOrderService)
        {
            _appointmentService = appointmentService;
            _partOrderService = partOrderService;
        }

        [HttpGet]
        public IActionResult GetOrders([FromQuery] string? type)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int customerId))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                }

                var list = new List<CustomerOrderDto>();

                using var context = new CarShowroomContext();

                // 1. Maintenance appointments (confirmed, in-progress, completed, cancelled)
                if (string.IsNullOrEmpty(type) || type.Equals("Maintenance", StringComparison.OrdinalIgnoreCase))
                {
                    var appointments = context.MaintenanceAppointments
                        .Include(a => a.AppointmentDetails)
                            .ThenInclude(d => d.Service)
                        .Include(a => a.AppointmentDetails)
                            .ThenInclude(d => d.Package)
                        .Include(a => a.ConsumedParts)
                        .Where(a => a.CustomerId == customerId && a.Status != "Pending")
                        .ToList();

                    var invoiceIds = appointments.Where(a => a.MasterInvoiceId.HasValue).Select(a => a.MasterInvoiceId!.Value).ToList();
                    var invoices = context.MasterInvoices.Where(m => invoiceIds.Contains(m.MasterInvoiceId)).ToDictionary(m => m.MasterInvoiceId);

                    foreach (var app in appointments)
                    {
                        MasterInvoice? masterInvoice = null;
                        if (app.MasterInvoiceId.HasValue)
                        {
                            invoices.TryGetValue(app.MasterInvoiceId.Value, out masterInvoice);
                        }

                        decimal totalAmount = 0;
                        bool isEstimated = true;

                        if (masterInvoice != null)
                        {
                            totalAmount = masterInvoice.TotalAmount;
                            isEstimated = false;
                        }
                        else
                        {
                            decimal detailsTotal = app.AppointmentDetails?.Sum(d => d.UnitPrice * d.Quantity) ?? 0;
                            decimal partsTotal = app.ConsumedParts?.Where(p => p.ApprovedByCustomer).Sum(p => p.UnitPrice * p.Quantity) ?? 0;
                            decimal extraFee = 0;
                            if (!string.IsNullOrEmpty(app.Note))
                            {
                                var match = System.Text.RegularExpressions.Regex.Match(app.Note, @"\[PhiPhatSinh:\s*(\d+)\]");
                                if (match.Success)
                                {
                                    decimal.TryParse(match.Groups[1].Value, out extraFee);
                                }
                            }
                            totalAmount = detailsTotal + partsTotal + extraFee;
                            isEstimated = true;
                        }

                        var serviceNames = app.AppointmentDetails
                            .Select(d => d.Service != null ? d.Service.ServiceName : (d.Package != null ? d.Package.PackageName : "Dịch vụ bảo dưỡng"))
                            .ToList();
                        string summary = string.Join(", ", serviceNames);
                        if (string.IsNullOrEmpty(summary))
                        {
                            summary = "Dịch vụ bảo dưỡng";
                        }

                        list.Add(new CustomerOrderDto
                        {
                            OrderCode = "SV-" + app.AppointmentId.ToString("D4"),
                            OrderType = "Maintenance",
                            SourceId = app.AppointmentId,
                            CreatedAt = app.CreatedAt,
                            Summary = summary,
                            TotalAmount = totalAmount,
                            IsEstimatedAmount = isEstimated,
                            ProcessingStatus = app.Status,
                            PaymentStatus = masterInvoice?.PaymentStatus ?? "",
                            AppointmentDateTime = app.AppointmentDate.ToDateTime(app.AppointmentTime),
                            DeliveryMethod = null
                        });
                    }
                }

                // 2. Part Orders
                if (string.IsNullOrEmpty(type) || type.Equals("Part", StringComparison.OrdinalIgnoreCase))
                {
                    var partOrders = context.PartOrders
                        .Include(o => o.PartOrderDetails)
                            .ThenInclude(od => od.Part)
                        .Where(o => o.CustomerId == customerId)
                        .ToList();

                    var invoiceIds = partOrders.Where(o => o.MasterInvoiceId.HasValue).Select(o => o.MasterInvoiceId!.Value).ToList();
                    var invoices = context.MasterInvoices.Where(m => invoiceIds.Contains(m.MasterInvoiceId)).ToDictionary(m => m.MasterInvoiceId);

                    foreach (var order in partOrders)
                    {
                        MasterInvoice? masterInvoice = null;
                        if (order.MasterInvoiceId.HasValue)
                        {
                            invoices.TryGetValue(order.MasterInvoiceId.Value, out masterInvoice);
                        }

                        string summary = "";
                        if (order.PartOrderDetails.Any())
                        {
                            var firstDetail = order.PartOrderDetails.First();
                            string partName = firstDetail.Part != null ? firstDetail.Part.PartName : "Phụ tùng";
                            int count = order.PartOrderDetails.Count;
                            if (count > 1)
                            {
                                summary = $"{partName} × {firstDetail.Quantity} và {count - 1} sản phẩm khác";
                            }
                            else
                            {
                                summary = $"{partName} × {firstDetail.Quantity}";
                            }
                        }
                        else
                        {
                            summary = "Đơn phụ tùng lẻ";
                        }

                        list.Add(new CustomerOrderDto
                        {
                            OrderCode = "PT-" + order.OrderId.ToString("D4"),
                            OrderType = "Part",
                            SourceId = order.OrderId,
                            CreatedAt = order.CreatedAt,
                            Summary = summary,
                            TotalAmount = order.TotalAmount,
                            IsEstimatedAmount = false,
                            ProcessingStatus = (order.Status == "Shipping" && order.ShippingAddress != null && order.ShippingAddress.Contains("[Received]")) ? "Delivered" : order.Status,
                            PaymentStatus = masterInvoice?.PaymentStatus ?? "",
                            AppointmentDateTime = null,
                            DeliveryMethod = order.DeliveryMethod
                        });
                    }
                }

                return Ok(list.OrderByDescending(o => o.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("Maintenance/{id}")]
        public IActionResult GetMaintenanceDetail(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int customerId))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                }

                var appDto = _appointmentService.GetAppointmentById(id);
                if (appDto == null)
                {
                    return NotFound(new { message = "Không tìm thấy lịch hẹn." });
                }

                if (appDto.CustomerId != customerId)
                {
                    return Forbid();
                }

                using var context = new CarShowroomContext();
                var appEntity = context.MaintenanceAppointments.FirstOrDefault(a => a.AppointmentId == id);
                MasterInvoice? masterInvoice = null;
                if (appEntity != null && appEntity.MasterInvoiceId.HasValue)
                {
                    masterInvoice = context.MasterInvoices.FirstOrDefault(m => m.MasterInvoiceId == appEntity.MasterInvoiceId.Value);
                }

                return Ok(new
                {
                    Appointment = appDto,
                    Invoice = masterInvoice
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("Part/{id}")]
        public IActionResult GetPartOrderDetail(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int customerId))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                }

                var order = _partOrderService.GetOrderById(id);
                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                if (order.CustomerId != customerId)
                {
                    return Forbid();
                }

                using var context = new CarShowroomContext();
                MasterInvoice? masterInvoice = null;
                if (order.MasterInvoiceId.HasValue)
                {
                    masterInvoice = context.MasterInvoices.FirstOrDefault(m => m.MasterInvoiceId == order.MasterInvoiceId.Value);
                }

                if (order.Status == "Shipping" && order.ShippingAddress != null && order.ShippingAddress.Contains("[Received]"))
                {
                    order.Status = "Delivered";
                    order.ShippingAddress = order.ShippingAddress.Replace(" [Received]", "");
                }

                return Ok(new
                {
                    Order = order,
                    Invoice = masterInvoice
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("pay/{type}/{id}")]
        public IActionResult PayOrder(string type, int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int customerId))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                }

                using var context = new CarShowroomContext();

                if (type.Equals("Maintenance", StringComparison.OrdinalIgnoreCase))
                {
                    var app = context.MaintenanceAppointments.FirstOrDefault(a => a.AppointmentId == id && a.CustomerId == customerId);
                    if (app == null) return NotFound(new { message = "Không tìm thấy lịch hẹn hoặc bạn không có quyền." });

                    app.IsPaid = true;
                    app.UpdatedAt = DateTime.Now;

                    if (app.MasterInvoiceId.HasValue)
                    {
                        var masterInvoice = context.MasterInvoices.Find(app.MasterInvoiceId.Value);
                        if (masterInvoice != null)
                        {
                            masterInvoice.PaymentStatus = "Paid";
                            masterInvoice.InvoiceStatus = "Completed";
                            masterInvoice.PaidAt = DateTime.Now;
                            masterInvoice.UpdatedAt = DateTime.Now;
                        }
                    }
                    context.SaveChanges();
                    return Ok(new { success = true, message = "Thanh toán dịch vụ bảo dưỡng thành công." });
                }
                else if (type.Equals("Part", StringComparison.OrdinalIgnoreCase))
                {
                    var order = context.PartOrders.FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId);
                    if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng hoặc bạn không có quyền." });

                    if (order.MasterInvoiceId.HasValue)
                    {
                        var masterInvoice = context.MasterInvoices.Find(order.MasterInvoiceId.Value);
                        if (masterInvoice != null)
                        {
                            masterInvoice.PaymentStatus = "Paid";
                            masterInvoice.InvoiceStatus = "Completed";
                            masterInvoice.PaidAt = DateTime.Now;
                            masterInvoice.UpdatedAt = DateTime.Now;
                        }
                    }
                    context.SaveChanges();
                    return Ok(new { success = true, message = "Thanh toán đơn phụ tùng thành công." });
                }

                return BadRequest(new { message = "Loại đơn hàng không hợp lệ." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("cancel/{type}/{id}")]
        public IActionResult CancelOrder(string type, int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int customerId))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                }

                using var context = new CarShowroomContext();

                if (type.Equals("Part", StringComparison.OrdinalIgnoreCase))
                {
                    var order = context.PartOrders.FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId);
                    if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng hoặc bạn không có quyền." });

                    if (order.Status != "Pending")
                    {
                        return BadRequest(new { message = "Đơn hàng đã được xác nhận hoặc đang xử lý, không thể tự hủy." });
                    }

                    order.Status = "Cancelled";
                    order.UpdatedAt = DateTime.Now;

                    if (order.MasterInvoiceId.HasValue)
                    {
                        var masterInvoice = context.MasterInvoices.Find(order.MasterInvoiceId.Value);
                        if (masterInvoice != null)
                        {
                            masterInvoice.InvoiceStatus = "Cancelled";
                            masterInvoice.UpdatedAt = DateTime.Now;
                        }
                    }
                    context.SaveChanges();
                    return Ok(new { success = true, message = "Hủy đơn hàng phụ tùng thành công." });
                }

                return BadRequest(new { message = "Loại đơn hàng không hợp lệ." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("receive/Part/{id}")]
        public IActionResult ReceivePartOrder(int id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int customerId))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                }

                using var context = new CarShowroomContext();
                var order = context.PartOrders.FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId);
                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng hoặc bạn không có quyền." });

                if (order.Status != "Shipping")
                {
                    return BadRequest(new { message = "Đơn hàng phải ở trạng thái đang giao mới có thể xác nhận đã nhận." });
                }

                order.ShippingAddress = (order.ShippingAddress ?? "").Trim();
                if (!order.ShippingAddress.Contains("[Received]"))
                {
                    order.ShippingAddress += " [Received]";
                }
                order.UpdatedAt = DateTime.Now;
                context.SaveChanges();
                return Ok(new { success = true, message = "Xác nhận nhận hàng thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
