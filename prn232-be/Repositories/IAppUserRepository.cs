using BusinessObjects.Models;

namespace Repositories
{
    public interface IAppUserRepository
    {
        AppUser GetUserByEmail(string email);
        AppUser GetUserById(int id);
        void AddUser(AppUser user);
        void UpdateUser(AppUser user);
    }
}
