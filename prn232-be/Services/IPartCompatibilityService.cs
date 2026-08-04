using BusinessObjects.DTOs;

namespace Services
{
    public interface IPartCompatibilityService
    {
        CompatibilityResultDto CheckCompatibility(string licensePlate, string partCode);
    }
}
