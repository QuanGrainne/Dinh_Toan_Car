using Microsoft.AspNetCore.Mvc;
using BusinessObjects.DTOs;
using Services;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentConsumedPartsController : ControllerBase
    {
        private readonly IAppointmentConsumedPartService _service;

        public AppointmentConsumedPartsController(IAppointmentConsumedPartService service)
        {
            _service = service;
        }

        [HttpPost("report-incurred")]
        public ActionResult<ApiResponse<string>> ReportIncurredPart([FromBody] IncurredPartReportDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse<object>(false, "Dữ liệu không hợp lệ", ModelState));

            try
            {
                _service.ReportIncurredPart(dto);
                return Ok(new ApiResponse<string>(true, "Đã báo cáo phụ tùng phát sinh, chờ khách hàng phê duyệt."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpPost("add-part")]
        public ActionResult<ApiResponse<string>> AddPart([FromBody] IncurredPartReportDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse<object>(false, "Dữ liệu không hợp lệ", ModelState));

            try
            {
                _service.AddPart(dto);
                return Ok(new ApiResponse<string>(true, "Đã thêm phụ tùng thành công."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpPost("approve")]
        public ActionResult<ApiResponse<string>> ApproveIncurredPart([FromBody] IncurredPartApprovalDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse<object>(false, "Dữ liệu không hợp lệ", ModelState));

            try
            {
                _service.ApproveIncurredPart(dto);
                var message = dto.IsApproved ? "Đã phê duyệt và cập nhật tồn kho phụ tùng." : "Đã từ chối phụ tùng phát sinh.";
                return Ok(new ApiResponse<string>(true, message));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpDelete("remove/{consumedPartId}")]
        public ActionResult<ApiResponse<string>> RemoveIncurredPart(int consumedPartId)
        {
            try
            {
                _service.RemoveIncurredPart(consumedPartId);
                return Ok(new ApiResponse<string>(true, "Đã hủy phụ tùng thành công."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }
    }
}
