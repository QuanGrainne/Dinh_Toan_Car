using BusinessObjects.DTOs;
using DataAccessObjects;

namespace Repositories
{
    public class PartCompatibilityRepository : IPartCompatibilityRepository
    {
        public CompatibilityResultDto CheckCompatibility(string licensePlate, string partCode) =>
            PartCompatibilityDAO.Instance.CheckCompatibility(licensePlate, partCode);
    }
}
