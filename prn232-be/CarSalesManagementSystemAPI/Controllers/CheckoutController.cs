using System.Security.Claims;
using BusinessObjects.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace CarSalesManagementSystemAPI.Controllers;

/// <summary>
/// Nhân viên lập hóa đơn tổng (MasterInvoice) cho khách: mua lẻ 1 module hoặc GỘP nhiều module
/// (xe + phụ tùng + dịch vụ) vào một hóa đơn, kèm sinh mã captcha đặt cọc/mua đứt.
/// </summary>
[ApiController]
[Route("api/checkout")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _service;

    public CheckoutController(ICheckoutService service)
    {
        _service = service;
    }

    /// <summary>Nhân viên lập hóa đơn tổng (sinh mã ngay).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public IActionResult Create([FromBody] CheckoutDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(raw, out var staffId)) return Unauthorized();

        var result = _service.CreateInvoice(dto, staffId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Khách tự lập hóa đơn tổng từ giỏ hàng (self-service). Luôn dùng CustomerId của chính khách,
    /// KHÔNG giữ chỗ xe và KHÔNG sinh captcha (nhân viên cấp mã sau). Cho phép mọi user đăng nhập.
    /// </summary>
    [HttpPost("customer")]
    [Authorize]
    public IActionResult CreateForCustomer([FromBody] CheckoutDto dto)
    {
        if (dto == null) return BadRequest(ServiceResult.Fail("Dữ liệu không hợp lệ."));

        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(raw, out var customerId)) return Unauthorized();

        dto.CustomerId = customerId; // ép về chính khách hàng đang đăng nhập
        var result = _service.CreateInvoice(dto, customerId, selfService: true);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
