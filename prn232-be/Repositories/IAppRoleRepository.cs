using BusinessObjects.Models;

namespace Repositories
{
    public interface IAppRoleRepository
    {
        AppRole GetRoleById(int id);
        AppRole GetRoleByName(string name);
    }
}
