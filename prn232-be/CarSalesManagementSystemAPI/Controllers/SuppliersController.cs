using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Services;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _service;

        public SuppliersController(ISupplierService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public ActionResult<IQueryable<Supplier>> Get()
        {
            try
            {
                var suppliers = _service.GetAllSuppliers();
                return Ok(suppliers.AsQueryable());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Supplier> GetById(int id)
        {
            try
            {
                var supplier = _service.GetSupplierById(id);
                if (supplier == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhà cung cấp yêu cầu." });
                }
                return Ok(supplier);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Post([FromBody] Supplier supplier)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (supplier.SupplierId > 0)
                {
                    var existing = _service.GetSupplierById(supplier.SupplierId);
                    if (existing != null)
                    {
                        supplier.CreatedAt = existing.CreatedAt;
                        supplier.UpdatedAt = DateTime.Now;
                        _service.UpdateSupplier(supplier);
                        return Ok(new { success = true, message = "Cập nhật nhà cung cấp thành công.", data = supplier });
                    }
                }

                supplier.SupplierId = 0;
                supplier.CreatedAt = DateTime.Now;
                _service.AddSupplier(supplier);
                return CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId }, supplier);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Put(int id, [FromBody] Supplier supplier)
        {
            try
            {
                if (id != supplier.SupplierId)
                {
                    return BadRequest(new { message = "Mã ID nhà cung cấp không khớp." });
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var existing = _service.GetSupplierById(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhà cung cấp cần cập nhật." });
                }
                supplier.CreatedAt = existing.CreatedAt;
                supplier.UpdatedAt = DateTime.Now;
                _service.UpdateSupplier(supplier);
                return Ok(new { success = true, message = "Cập nhật nhà cung cấp thành công." });
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
                var supplier = _service.GetSupplierById(id);
                if (supplier == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhà cung cấp cần xóa." });
                }
                _service.DeleteSupplier(id);
                return Ok(new { success = true, message = "Xóa nhà cung cấp thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
