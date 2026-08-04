using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class AppRoleRepository : IAppRoleRepository
    {
        public AppRole GetRoleById(int id) => AppRoleDAO.Instance.GetRoleById(id);
        public AppRole GetRoleByName(string name) => AppRoleDAO.Instance.GetRoleByName(name);
    }
}
