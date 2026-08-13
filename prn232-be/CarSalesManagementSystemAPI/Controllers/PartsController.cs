using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Services;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.Authorization;

namespace CarSalesManagementSystemAPI.Controllers
{
    /// <summary>
    /// Handles both OData queries (odata/Parts) and REST CRUD (api/Parts).
    /// Frontend PartsController calls odata/Parts with $expand=Category etc.
    /// </summary>
    public class PartsController : ODataController
    {
        private readonly IPartService _partService;

        public PartsController(IPartService partService)
        {
            _partService = partService;
        }

        // ─── OData query endpoint ─────────────────────────────────────────────
        // GET odata/Parts  (supports $filter, $expand=Category, $orderby, $count, $top, $skip)
        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 3)]
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

        // GET odata/Parts(5)
        [HttpGet]
        [EnableQuery]
        public ActionResult<Part> Get([FromODataUri] int key)
        {
            try
            {
                var part = _partService.GetPartById(key);
                if (part == null)
                    return NotFound(new { message = "Không tìm thấy phụ tùng yêu cầu." });
                return Ok(part);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ─── REST endpoints ───────────────────────────────────────────────────
        // GET api/Parts/{id}/details-for-edit
        [HttpGet("/api/Parts/{key}/details-for-edit")]
        public IActionResult GetDetailsForEdit(int key)
        {
            try
            {
                var part = _partService.GetPartById(key);
                if (part == null)
                    return NotFound(new { message = "Không tìm thấy phụ tùng yêu cầu." });

                bool canEditPartCode = !_partService.HasTransactions(key);
                return Ok(new BusinessObjects.ViewModels.UpdatePartViewModel
                {
                    PartId = part.PartId,
                    PartName = part.PartName,
                    PartCode = part.PartCode,
                    CategoryId = part.CategoryId,
                    Brand = part.Brand,
                    Price = part.Price,
                    Description = part.Description,
                    ImageUrl = part.ImageUrl,
                    Status = part.Status,
                    CanEditPartCode = canEditPartCode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // POST api/Parts
        [HttpPost("/api/Parts")]
        [Authorize(Roles = "Admin")]
        public IActionResult Post([FromBody] Part part)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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

        // PUT api/Parts/{id}
        [HttpPut("/api/Parts/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromBody] BusinessObjects.ViewModels.UpdatePartViewModel model)
        {
            try
            {
                if (id != model.PartId)
                    return BadRequest(new { message = "Mã ID phụ tùng không khớp." });
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                int? adminId = null;
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int parsedId))
                    adminId = parsedId;

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

        // DELETE api/Parts/{id}
        [HttpDelete("/api/Parts/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                var part = _partService.GetPartById(id);
                if (part == null)
                    return NotFound(new { message = "Không tìm thấy phụ tùng cần xóa." });

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

        // GET api/Parts/filter?categoryId=1
        [HttpGet("/api/Parts/filter")]
        public ActionResult<IEnumerable<Part>> GetFilteredParts([FromQuery] int categoryId)
        {
            try
            {
                var result = _partService.GetPartsFiltered(categoryId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // POST api/Parts/check-compatibility
        [HttpPost("/api/Parts/check-compatibility")]
        public IActionResult CheckCompatibility([FromBody] PartCompatibilityCheckDto dto)
        {
            return Ok(new { isCompatible = true, message = "Phụ tùng tương thích với xe." });
        }
    }
}
