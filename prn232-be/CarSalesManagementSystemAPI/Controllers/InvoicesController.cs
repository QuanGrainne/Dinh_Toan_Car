using System.Security.Claims;
using BusinessObjects.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace CarSalesManagementSystemAPI.Controllers;

/// <summary>
/// Hóa đơn tổng (MasterInvoice) &amp; thanh toán DÙNG CHUNG cho cả 3 module + hóa đơn gộp.
/// Nhân viên sinh mã captcha (đặt cọc/mua đứt); khách hàng nhập mã để xác thực hóa đơn.
/// </summary>
[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IMasterInvoicePaymentService _service;

    public InvoicesController(IMasterInvoicePaymentService service)
    {
        _service = service;
    }

    private int? CurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : (int?)null;
    }

    private bool IsStaffOrAdmin() => User.IsInRole("Admin") || User.IsInRole("Staff");

    // ---------- NHÂN VIÊN: sinh mã ----------

    [HttpPost("deposit-captcha")]
    [Authorize(Roles = "Admin,Staff")]
    public IActionResult GenerateDepositCaptcha([FromBody] GenerateDepositCaptchaDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var staffId = CurrentUserId();
        if (staffId == null) return Unauthorized();
        var result = _service.GenerateDepositCaptcha(dto, staffId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{masterInvoiceId:int}/final-captcha")]
    [Authorize(Roles = "Admin,Staff")]
    public IActionResult GenerateFinalCaptcha(int masterInvoiceId)
    {
        var staffId = CurrentUserId();
        if (staffId == null) return Unauthorized();
        var result = _service.GenerateFinalCaptcha(masterInvoiceId, staffId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("deposits/release-expired")]
    [Authorize(Roles = "Admin,Staff")]
    public IActionResult ReleaseExpired()
    {
        var count = _service.ReleaseExpiredDeposits();
        return Ok(ServiceResult.Ok($"Đã giải phóng {count} hóa đơn cọc hết hạn.", new { released = count }));
    }

    // ---------- KHÁCH HÀNG: xác thực ----------

    [HttpPost("verify/deposit")]
    [Authorize]
    public IActionResult VerifyDeposit([FromBody] VerifyCaptchaDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();
        var result = _service.VerifyDeposit(dto, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("verify/final")]
    [Authorize]
    public IActionResult VerifyFinal([FromBody] VerifyCaptchaDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();
        var result = _service.VerifyFinal(dto, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ---------- Truy vấn ----------

    [HttpGet("{masterInvoiceId:int}")]
    [Authorize]
    public IActionResult GetInvoice(int masterInvoiceId)
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();
        bool staff = IsStaffOrAdmin();
        var invoice = _service.GetInvoice(masterInvoiceId, includeCaptcha: staff);
        if (invoice == null) return NotFound(ServiceResult.Fail("Không tìm thấy hóa đơn."));
        if (!staff && invoice.CustomerId != userId.Value) return Forbid();
        return Ok(invoice);
    }

    /// <summary>Danh sách hóa đơn. Admin/Staff xem tất cả (kèm mã); khách chỉ xem của mình. Lọc theo ?type=Car|Part|Service|Combined.</summary>
    [HttpGet]
    [Authorize]
    public IActionResult GetInvoices([FromQuery] string? type)
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();
        bool staff = IsStaffOrAdmin();
        int? filter = staff ? null : userId;
        return Ok(_service.GetInvoices(filter, type, includeCaptcha: staff));
    }
}
