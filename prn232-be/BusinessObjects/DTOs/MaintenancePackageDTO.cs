using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    public class MaintenancePackageDTO
    {
        public int PackageId { get; set; }

        [Required(ErrorMessage = "Ten goi khong duoc trong")]
        [StringLength(150, ErrorMessage = "Ten goi khong qua 150 ky tu")]
        public string PackageName { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Gia khong hop le")]
        public decimal PackagePrice { get; set; }

        public string Status { get; set; } = "Available";
        public DateTime? CreatedAt { get; set; }

        // Danh sách dịch vụ trong gói (khi đọc)
        public List<ServiceSummaryDTO> Services { get; set; } = new();

        // Danh sách serviceIds khi tạo/cập nhật gói
        public List<int> ServiceIds { get; set; } = new();

        // Computed fields (tính toán từ Services)
        public decimal TotalBasePrice => Services.Sum(s => s.BasePrice);
        public decimal SavingAmount => TotalBasePrice > 0 ? TotalBasePrice - PackagePrice : 0;
        public int TotalDurationMinutes => Services.Sum(s => s.EstimatedDurationMinutes);
    }
}
