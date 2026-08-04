using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.DTOs;
using Services;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _service;

        public ServicesController(IServiceService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<ServiceDTO>>> Get()
        {
            var data = _service.GetAllServices().ToList();
            return Ok(new ApiResponse<IEnumerable<ServiceDTO>>(true, "Success", data));
        }

        [HttpGet("available")]
        public ActionResult<ApiResponse<IEnumerable<ServiceDTO>>> GetAvailable()
        {
            var data = _service.GetAvailableServices().ToList();
            return Ok(new ApiResponse<IEnumerable<ServiceDTO>>(true, "Success", data));
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResponse<ServiceDTO>> Get(int id)
        {
            var service = _service.GetServiceById(id);
            if (service == null)
            {
                return NotFound(new ApiResponse<ServiceDTO>(false, "Không tìm thấy"));
            }
            return Ok(new ApiResponse<ServiceDTO>(true, "Success", service));
        }

        [HttpPost]
        public ActionResult<ApiResponse<ServiceDTO>> Post([FromBody] ServiceDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse<object>(false, "Dữ liệu không hợp lệ", ModelState));
            
            _service.AddService(dto);
            return Ok(new ApiResponse<string>(true, "Thêm thành công"));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResponse<string>> Put(int id, [FromBody] ServiceDTO dto)
        {
            if (id != dto.ServiceId) return BadRequest(new ApiResponse<string>(false, "ID không hợp lệ"));
            
            var service = _service.GetServiceById(id);
            if (service == null) return NotFound(new ApiResponse<string>(false, "Không tìm thấy"));

            _service.UpdateService(dto);
            return Ok(new ApiResponse<string>(true, "Cập nhật thành công"));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResponse<string>> Delete(int id)
        {
            try
            {
                _service.DeleteService(id);
                return Ok(new ApiResponse<string>(true, "Xóa thành công"));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }
    }
}
