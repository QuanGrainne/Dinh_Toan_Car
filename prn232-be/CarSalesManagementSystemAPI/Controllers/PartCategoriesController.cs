using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Services;
using Microsoft.AspNetCore.OData.Query;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartCategoriesController : ControllerBase
    {
        private readonly IPartCategoryService _service;

        public PartCategoriesController(IPartCategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public ActionResult<IQueryable<PartCategory>> Get()
        {
            try
            {
                var categories = _service.GetAllCategories();
                return Ok(categories.AsQueryable());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<PartCategory> GetById(int id)
        {
            try
            {
                var category = _service.GetCategoryById(id);
                if (category == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh mục yêu cầu." });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Post([FromBody] PartCategory category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (category.CategoryId > 0)
                {
                    var existing = _service.GetCategoryById(category.CategoryId);
                    if (existing != null)
                    {
                        _service.UpdateCategory(category);
                        return Ok(new { success = true, message = "Cập nhật danh mục thành công.", data = category });
                    }
                }

                category.CategoryId = 0;
                category.CreatedAt = DateTime.Now;
                _service.AddCategory(category);
                return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromBody] PartCategory category)
        {
            try
            {
                if (id != category.CategoryId)
                {
                    return BadRequest(new { message = "Mã ID danh mục không khớp." });
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existing = _service.GetCategoryById(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh mục cần cập nhật." });
                }

                _service.UpdateCategory(category);
                return Ok(new { success = true, message = "Cập nhật danh mục thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                var existing = _service.GetCategoryById(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh mục cần xóa." });
                }

                _service.DeleteCategory(id);
                return Ok(new { success = true, message = "Xóa danh mục thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
