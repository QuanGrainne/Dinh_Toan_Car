using BusinessObjects.DTOs;
using Repositories;

namespace Services
{
    public class PartCompatibilityService : IPartCompatibilityService
    {
        private readonly IPartCompatibilityRepository _repository;

        public PartCompatibilityService(IPartCompatibilityRepository repository)
        {
            _repository = repository;
        }

        public CompatibilityResultDto CheckCompatibility(string licensePlate, string partCode)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                return new CompatibilityResultDto
                {
                    PartCode = partCode,
                    PartName = "Không xác định",
                    IsCompatible = false,
                    Message = "Biển số xe không được để trống."
                };
            }
            if (string.IsNullOrWhiteSpace(partCode))
            {
                return new CompatibilityResultDto
                {
                    PartCode = "",
                    PartName = "Không xác định",
                    IsCompatible = false,
                    Message = "Mã phụ tùng không được để trống."
                };
            }

            return _repository.CheckCompatibility(licensePlate.Trim(), partCode.Trim());
        }
    }
}
