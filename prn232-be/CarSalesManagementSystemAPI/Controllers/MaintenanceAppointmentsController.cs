using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Services;

namespace CarSalesManagementSystemAPI.Controllers
{
    public class UpdateStatusRequest
    {
        public string Status { get; set; } = null!;
        public string? Reason { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceAppointmentsController : ControllerBase
    {
        private readonly IMaintenanceAppointmentService _service;

        public MaintenanceAppointmentsController(IMaintenanceAppointmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<MaintenanceAppointmentDTO>>> Get()
        {
            var data = _service.GetAllAppointments().ToList();
            return Ok(new ApiResponse<IEnumerable<MaintenanceAppointmentDTO>>(true, "Success", data));
        }

        [HttpGet("customer/{customerId}")]
        public ActionResult<ApiResponse<IEnumerable<MaintenanceAppointmentDTO>>> GetByCustomer(int customerId)
        {
            var data = _service.GetAppointmentsByCustomerId(customerId).ToList();
            return Ok(new ApiResponse<IEnumerable<MaintenanceAppointmentDTO>>(true, "Success", data));
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResponse<MaintenanceAppointmentDTO>> Get(int id)
        {
            var appointment = _service.GetAppointmentById(id);
            if (appointment == null)
            {
                return NotFound(new ApiResponse<MaintenanceAppointmentDTO>(false, "Không tìm thấy"));
            }
            return Ok(new ApiResponse<MaintenanceAppointmentDTO>(true, "Success", appointment));
        }

        [HttpPost("{customerId}")]
        public ActionResult<ApiResponse<MaintenanceAppointmentDTO>> Post(int customerId, [FromBody] CreateAppointmentDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse<object>(false, "Dữ liệu không hợp lệ", ModelState));

            try
            {
                var appointment = _service.CreateAppointment(customerId, dto);
                return Ok(new ApiResponse<MaintenanceAppointmentDTO>(true, "Thêm thành công", appointment));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpPost("create-with-details")]
        public ActionResult<ApiResponse<MaintenanceAppointmentDTO>> CreateWithDetails([FromBody] CreateAppointmentDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse<object>(false, "Dữ liệu không hợp lệ", ModelState));

            try
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value
                             ?? User.FindFirst("nameid")?.Value;
                             
                int customerId = dto.CustomerId ?? 1; // Default guest fallback ID if not provided in DTO
                
                if (dto.CustomerId == null && !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int parsedId))
                {
                    customerId = parsedId;
                }

                var appointment = _service.CreateAppointment(customerId, dto);
                return Ok(new ApiResponse<MaintenanceAppointmentDTO>(true, "Thêm thành công", appointment));
            }
            catch (System.Exception ex)
            {
                var msg = ex.Message;
                if (ex.InnerException != null) msg += " INNER: " + ex.InnerException.Message;
                return BadRequest(new ApiResponse<string>(false, msg));
            }
        }

        [HttpPut("{id}/status")]
        public ActionResult<ApiResponse<string>> UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
        {
            _service.UpdateAppointmentStatus(id, req.Status, req.Reason);
            return Ok(new ApiResponse<string>(true, "Cập nhật thành công"));
        }

        [HttpPut("{id}/pay")]
        public ActionResult<ApiResponse<string>> UpdatePaymentStatus(int id)
        {
            _service.UpdateAppointmentPaymentStatus(id, true);
            return Ok(new ApiResponse<string>(true, "Xác nhận thanh toán thành công"));
        }

        public class UpdateExtraFeeRequest
        {
            public decimal ExtraFee { get; set; }
        }

        [HttpPut("{id}/extrafee")]
        public ActionResult<ApiResponse<string>> UpdateExtraFee(int id, [FromBody] UpdateExtraFeeRequest req)
        {
            _service.UpdateAppointmentExtraFee(id, req.ExtraFee);
            return Ok(new ApiResponse<string>(true, "Lưu phí phát sinh thành công"));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResponse<string>> Delete(int id)
        {
            _service.DeleteAppointment(id);
            return Ok(new ApiResponse<string>(true, "Xóa thành công"));
        }
    }
}
