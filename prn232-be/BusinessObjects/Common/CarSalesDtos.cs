using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Common;

/// <summary>Kết quả nghiệp vụ chung, thống nhất cho tầng Service của module ô tô.</summary>
public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static ServiceResult Ok(string message, object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    public static ServiceResult Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>Khách hàng gửi yêu cầu mua/quan tâm một chiếc xe. CustomerId lấy từ JWT.</summary>
public class CreatePurchaseRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã xe không hợp lệ.")]
    public int CarId { get; set; }

    [Required(ErrorMessage = "Tên khách hàng không được trống.")]
    [StringLength(100)]
    public string CustomerName { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại không được trống.")]
    [StringLength(20)]
    public string CustomerPhone { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100)]
    public string? CustomerEmail { get; set; }

    [StringLength(1000)]
    public string? Message { get; set; }
}

/// <summary>Khách nhập mã captcha do nhân viên cung cấp để xác thực hóa đơn (dùng chung mọi module).</summary>
public class VerifyCaptchaDto
{
    [Range(1, int.MaxValue)]
    public int MasterInvoiceId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã xác thực.")]
    [StringLength(20)]
    public string CaptchaCode { get; set; } = null!;
}
