using System;

namespace BusinessObjects.DTOs
{
    // DTO kiểm tra độ tương thích của phụ tùng với xe khách
    public class PartCompatibilityCheckDto
    {
        public string LicensePlate { get; set; } = null!;
        public string PartCode { get; set; } = null!;
    }

    // DTO phản hồi khi kiểm tra tương thích
    public class CompatibilityResultDto
    {
        public string PartCode { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public bool IsCompatible { get; set; }
        public string Message { get; set; } = null!;
    }
}
