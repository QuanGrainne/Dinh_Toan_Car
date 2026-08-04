using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Services;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;
        private readonly IPartCompatibilityService _compatibilityService;

        public PartsController(IPartService partService, IPartCompatibilityService compatibilityService)
        {
            _partService = partService;
            _compatibilityService = compatibilityService;
        }

        [HttpGet]
        [EnableQuery]
        public ActionResult<IQueryable<Part>> Get()
        {
            try
            {
                var parts = _partService.GetAllParts();
                return Ok(parts.AsQueryable());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public ActionResult<Part> Get(int key)
        {
            try
            {
                var part = _partService.GetPartById(key);
                if (part == null)
                {
                    return NotFound(new { message = "Không tìm thấy phụ tùng yêu cầu." });
                }
                return Ok(part);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("{key}/details-for-edit")]
        public IActionResult GetDetailsForEdit(int key)
        {
            try
            {
                var part = _partService.GetPartById(key);
                if (part == null)
                {
                    return NotFound(new { message = "Không tìm thấy phụ tùng yêu cầu." });
                }
                bool canEditPartCode = !_partService.HasTransactions(key);
                return Ok(new BusinessObjects.ViewModels.UpdatePartViewModel
                {
                    PartId = part.PartId,
                    PartName = part.PartName,
                    PartCode = part.PartCode,
                    CategoryId = part.CategoryId,
                    Brand = part.Brand,
                    Price = part.Price,
                    MinStockLevel = part.MinStockLevel,
                    MaxStockLevel = part.MaxStockLevel,
                    UnitOfMeasure = part.UnitOfMeasure ?? "Cái",
                    WarehouseLocation = part.WarehouseLocation,
                    WarrantyMonths = part.WarrantyMonths,
                    Description = part.Description,
                    ImageUrl = part.ImageUrl,
                    Status = part.Status,
                    CurrentQuantity = part.Quantity,
                    CurrentExpiredAt = part.ExpiredAt,
                    CanEditPartCode = canEditPartCode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Post([FromBody] Part part)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                part.CreatedAt = DateTime.Now;
                _partService.AddPart(part);
                return CreatedAtAction(nameof(Get), new { key = part.PartId }, part);
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
        [Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromBody] BusinessObjects.ViewModels.UpdatePartViewModel model)
        {
            try
            {
                if (id != model.PartId)
                {
                    return BadRequest(new { message = "Mã ID phụ tùng không khớp." });
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int? adminId = null;
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int parsedId))
                {
                    adminId = parsedId;
                }

                _partService.UpdatePartMetadata(model, adminId);
                return Ok(new { success = true, message = "Cập nhật phụ tùng thành công." });
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


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                var part = _partService.GetPartById(id);
                if (part == null)
                {
                    return NotFound(new { message = "Không tìm thấy phụ tùng cần xóa." });
                }
                _partService.DeletePart(id);
                return Ok(new { success = true, message = "Xóa phụ tùng thành công." });
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

        [HttpPost("check-compatibility")]
        public IActionResult CheckCompatibility([FromBody] PartCompatibilityCheckDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = _compatibilityService.CheckCompatibility(dto.LicensePlate, dto.PartCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("filter")]
        public ActionResult<IEnumerable<Part>> GetFilteredParts([FromQuery] int categoryId, [FromQuery] int supplierId)
        {
            try
            {
                var result = _partService.GetPartsFiltered(categoryId, supplierId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
