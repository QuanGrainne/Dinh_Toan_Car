using System.Security.Claims;
using BusinessObjects.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace CarSalesManagementSystemAPI.Controllers;

/// <summary>
/// Yêu cầu mua xe của khách hàng (module ô tô).
/// Sau khi khách gửi yêu cầu, nhân viên lập hóa đơn qua <c>POST /api/checkout</c> (kèm dòng xe),
/// rồi thanh toán/đặt cọc/mua đứt qua <c>/api/invoices/*</c> — dùng chung cho cả 3 module.
/// </summary>
[ApiController]
[Route("api/car-sales")]
public class CarSalesController : ControllerBase
{
    private readonly ICarSalesService _service;

    public CarSalesController(ICarSalesService service)
    {
        _service = service;
    }

    private int? CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : (int?)null;
    }

    private bool IsStaffOrAdmin() => User.IsInRole("Admin") || User.IsInRole("Staff");

    /// <summary>Khách hàng gửi yêu cầu mua xe.</summary>
    [HttpPost("requests")]
    [Authorize]
    public IActionResult CreateRequest([FromBody] CreatePurchaseRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized(ServiceResult.Fail("Không xác định được người dùng."));

        var result = _service.CreatePurchaseRequest(dto, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Danh sách yêu cầu mua. Admin/Staff xem tất cả; khách chỉ xem của mình.</summary>
    [HttpGet("requests")]
    [Authorize]
    public IActionResult GetRequests()
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();
        int? filter = IsStaffOrAdmin() ? null : userId;
        return Ok(_service.GetPurchaseRequests(filter));
    }
}
