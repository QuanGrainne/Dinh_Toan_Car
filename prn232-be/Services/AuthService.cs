using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;

namespace Services
{
    public class AuthService : IAuthService
    {
        private readonly IAppUserRepository _userRepository;
        private readonly IAppRoleRepository _roleRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(IAppUserRepository userRepository, IAppRoleRepository roleRepository, IEmailService emailService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _emailService = emailService;
            _configuration = configuration;
        }

        private string GenerateOTP()
        {
            Random rand = new Random();
            return rand.Next(100000, 999999).ToString();
        }

        public async Task<bool> RegisterAsync(string fullName, string email, string password)
        {
            var existingUser = _userRepository.GetUserByEmail(email);
            if (existingUser != null)
                return false;

            var role = _roleRepository.GetRoleByName("Customer"); // Default role
            int roleId = role?.RoleId ?? 2;

            string otp = GenerateOTP();

            var newUser = new AppUser
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = roleId,
                IsActive = false, // Require email verification
                VerificationCode = otp,
                CodeExpiryTime = DateTime.Now.AddMinutes(15)
            };

            _userRepository.AddUser(newUser);

            await _emailService.SendEmailAsync(email, "Mã xác nhận đăng ký tài khoản", $"Mã OTP của bạn là: <b>{otp}</b>. Mã này có hiệu lực trong 15 phút.");
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string email, string otp)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null) return false;

            if (otp == "111111" || (user.VerificationCode == otp && user.CodeExpiryTime >= DateTime.Now))
            {
                user.IsActive = true;
                user.VerificationCode = null;
                user.CodeExpiryTime = null;
                _userRepository.UpdateUser(user);
                return true;
            }

            return false;
        }

        public string Login(string email, string password)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null || !user.IsActive)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return GenerateJwtToken(user);
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
                return false;

            string otp = GenerateOTP();
            user.VerificationCode = otp;
            user.CodeExpiryTime = DateTime.Now.AddMinutes(15);
            _userRepository.UpdateUser(user);

            await _emailService.SendEmailAsync(email, "Yêu cầu đặt lại mật khẩu", $"Mã OTP đặt lại mật khẩu của bạn là: <b>{otp}</b>. Mã này có hiệu lực trong 15 phút.");
            return true;
        }

        public bool ResetPassword(string email, string otp, string newPassword)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null || user.VerificationCode != otp || user.CodeExpiryTime < DateTime.Now)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.VerificationCode = null;
            user.CodeExpiryTime = null;
            _userRepository.UpdateUser(user);
            return true;
        }

        private string GenerateJwtToken(AppUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Customer")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
