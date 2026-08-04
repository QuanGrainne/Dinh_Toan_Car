using BusinessObjects.DTOs;

namespace Repositories
{
    public interface IPartCompatibilityRepository
    {
        CompatibilityResultDto CheckCompatibility(string licensePlate, string partCode);
    }
}
