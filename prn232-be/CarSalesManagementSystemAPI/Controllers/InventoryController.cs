using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.DTOs;
using Services;

using BusinessObjects.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IInventoryReceiptService _inventoryReceiptService;

        public InventoryController(
            IInventoryService inventoryService,
            IInventoryReceiptService inventoryReceiptService)
        {
            _inventoryService = inventoryService;
            _inventoryReceiptService = inventoryReceiptService;
        }

        [HttpPost("receipt")]
        public async Task<IActionResult> CreateReceipt([FromBody] InventoryReceiptCreateViewModel model, CancellationToken cancellationToken)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Extract User ID from JWT Token Claims
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính nhân viên." });
                }

                int staffId = int.Parse(userIdStr);
                var result = await _inventoryReceiptService.CreateReceiptAsync(model, staffId, cancellationToken);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok(new { success = true, message = result.Message, data = result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("adjust")]
        public IActionResult AdjustInventory([FromBody] InventoryAdjustmentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Unauthorized(new { message = "Không xác định được danh tính nhân viên." });
                }

                int staffId = int.Parse(userIdStr);
                _inventoryService.AdjustInventory(model, staffId);
                return Ok(new { success = true, message = "Điều chỉnh tồn kho thành công." });
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

        [HttpGet("transactions/{partId}")]
        public IActionResult GetTransactionsByPartId(int partId)
        {
            try
            {
                var transactions = _inventoryService.GetTransactionsByPartId(partId);
                var result = transactions.Select(t => new {
                    t.TransactionId,
                    t.PartId,
                    t.TransactionType,
                    t.Quantity,
                    t.ReferenceType,
                    t.ReferenceId,
                    t.StaffId,
                    StaffName = t.Staff != null ? t.Staff.FullName : "N/A",
                    t.Notes,
                    TransactionDate = t.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss")
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}

