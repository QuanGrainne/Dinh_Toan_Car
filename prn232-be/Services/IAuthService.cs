using System.Threading.Tasks;

namespace Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(string fullName, string email, string password);
        Task<bool> VerifyEmailAsync(string email, string otp);
        string Login(string email, string password);
        Task<bool> ForgotPasswordAsync(string email);
        bool ResetPassword(string email, string otp, string newPassword);
    }
}
