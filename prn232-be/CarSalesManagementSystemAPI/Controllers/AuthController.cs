using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAppUserRepository _userRepository;

        public AuthController(IAuthService authService, IAppUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        public class RegisterRequest
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            bool result = await _authService.RegisterAsync(request.FullName, request.Email, request.Password);
            if (!result)
                return BadRequest(new { Message = "Email da ton tai." });

            return Ok(new { Message = "Dang ky thanh cong! Vui long kiem tra Email de lay ma xac nhan OTP." });
        }

        public class VerifyRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Otp { get; set; } = string.Empty;
        }

        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyRequest request)
        {
            bool result = await _authService.VerifyEmailAsync(request.Email, request.Otp);
            if (!result)
                return BadRequest(new { Message = "Ma OTP khong hop le hoac da het han." });

            return Ok(new { Message = "Xac thuc Email thanh cong. Ban da co the dang nhap." });
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var token = _authService.Login(request.Email, request.Password);
            if (token == null)
                return Unauthorized(new { Message = "Email hoac Mat khau khong dung. Vui long kiem tra lai (Luu y: Ban phai xac nhan Email truoc khi dang nhap)." });

            return Ok(new { Token = token, Message = "Dang nhap thanh cong!" });
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            bool result = await _authService.ForgotPasswordAsync(request.Email);
            if (!result)
                return BadRequest(new { Message = "Khong tim thay tai khoan voi Email nay." });

            return Ok(new { Message = "Vui long kiem tra Email de lay ma OTP dat lai mat khau." });
        }

        public class ResetPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Otp { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            bool result = _authService.ResetPassword(request.Email, request.Otp, request.NewPassword);
            if (!result)
                return BadRequest(new { Message = "Ma OTP khong hop le hoac da het han." });

            return Ok(new { Message = "Dat lai mat khau thanh cong!" });
        }

        public class UpdatePhoneRequest
        {
            public string PhoneNumber { get; set; } = string.Empty;
        }

        [Authorize]
        [HttpGet("Me")]
        public IActionResult Me()
        {
            var user = GetCurrentUser();
            if (user == null)
                return Unauthorized(new { Message = "Khong xac dinh duoc nguoi dung hien tai." });

            return Ok(new
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role?.RoleName ?? "Customer"
            });
        }

        [Authorize]
        [HttpPut("Me/Phone")]
        public IActionResult UpdatePhone([FromBody] UpdatePhoneRequest request)
        {
            var user = GetCurrentUser();
            if (user == null)
                return Unauthorized(new { Message = "Khong xac dinh duoc nguoi dung hien tai." });

            var phoneNumber = request?.PhoneNumber?.Trim();
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest(new { Message = "Vui long nhap so dien thoai." });

            if (!Regex.IsMatch(phoneNumber, @"^[0-9+\-\s]{8,20}$"))
                return BadRequest(new { Message = "So dien thoai khong hop le." });

            user.PhoneNumber = phoneNumber;
            _userRepository.UpdateUser(user);

            return Ok(new
            {
                Message = "Cap nhat so dien thoai thanh cong.",
                PhoneNumber = user.PhoneNumber
            });
        }

        private BusinessObjects.Models.AppUser? GetCurrentUser()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdValue, out var userId))
                return null;

            return _userRepository.GetUserById(userId);
        }
    }
}
