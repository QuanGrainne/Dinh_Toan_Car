using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Services;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PartOrdersController : ControllerBase
    {
        private readonly IPartOrderService _orderService;

        public PartOrdersController(IPartOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [EnableQuery]
        public ActionResult<IQueryable<PartOrder>> Get()
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                }

                int userId = int.Parse(userIdStr);

                if (role == "Admin")
                {
                    var orders = _orderService.GetAllOrders();
                    return Ok(orders.AsQueryable());
                }
                else
                {
                    var orders = _orderService.GetOrdersByCustomerId(userId);
                    return Ok(orders.AsQueryable());
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public ActionResult<PartOrder> Get(int key)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                }

                int userId = int.Parse(userIdStr);
                var order = _orderService.GetOrderById(key);

                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng yêu cầu." });
                }

                if (role != "Admin" && order.CustomerId != userId)
                {
                    return Forbid();
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] PartOrder order)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Unauthorized(new { message = "Vui lòng đăng nhập để đặt hàng." });
                }

                order.CustomerId = int.Parse(userIdStr);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _orderService.AddOrder(order);
                return CreatedAtAction(nameof(Get), new { key = order.OrderId }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] PartOrder order)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính." });
                }

                int userId = int.Parse(userIdStr);

                var existingOrder = _orderService.GetOrderById(id);
                if (existingOrder == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng cần cập nhật." });
                }

                // If user is Admin, they can update anything (specifically status)
                // If user is Customer, they can only Cancel their own order if it is still Pending
                if (role != "Admin")
                {
                    if (existingOrder.CustomerId != userId)
                    {
                        return Forbid();
                    }

                    if (order.Status == "Cancelled")
                    {
                        if (existingOrder.Status != "Pending")
                        {
                            return BadRequest(new { message = "Chỉ có thể hủy đơn hàng khi trạng thái là Chờ duyệt (Pending)." });
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = "Khách hàng chỉ có quyền hủy đơn hàng." });
                    }
                }

                // Copy updated fields
                existingOrder.Status = order.Status;
                
                _orderService.UpdateOrder(existingOrder);
                return Ok(new { success = true, message = "Cập nhật đơn hàng thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
