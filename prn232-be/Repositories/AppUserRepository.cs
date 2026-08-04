using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class AppUserRepository : IAppUserRepository
    {
        public AppUser GetUserByEmail(string email) => AppUserDAO.Instance.GetUserByEmail(email);
        public AppUser GetUserById(int id) => AppUserDAO.Instance.GetUserById(id);
        public void AddUser(AppUser user) => AppUserDAO.Instance.AddUser(user);
        public void UpdateUser(AppUser user) => AppUserDAO.Instance.UpdateUser(user);
    }
}
