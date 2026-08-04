using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    public class MaintenanceAppointmentDTO
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public int CustomerCarId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = null!;

        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [Required]
        public DateOnly AppointmentDate { get; set; }

        [Required]
        public TimeOnly AppointmentTime { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = null!;
        
        public bool IsPaid { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation: xe của khách
        public CustomerCarDTO? CustomerCar { get; set; }

        // Navigation: danh sách gói/dịch vụ đặt
        public List<AppointmentDetailDTO> Details { get; set; } = new();

        // Navigation: phụ tùng đã sử dụng
        public List<ConsumedPartDTO> ConsumedParts { get; set; } = new();
    }

    // DTO để tạo booking mới
    public class CreateAppointmentDTO
    {
        public int? CustomerId { get; set; }

        public int CustomerCarId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = null!;

        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [Required]
        public DateOnly AppointmentDate { get; set; }

        [Required]
        public TimeOnly AppointmentTime { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        // Danh sách PackageId chọn
        public List<int> PackageIds { get; set; } = new();

        // Danh sách ServiceId chọn (dịch vụ lẻ)
        public List<int> ServiceIds { get; set; } = new();

        // Danh sách Parts chọn (mua kèm theo Giỏ hàng chung)
        public List<UnifiedPartItemDTO> PartItems { get; set; } = new();

        [StringLength(100)]
        public string? CarName { get; set; }

        [StringLength(50)]
        public string? LicensePlate { get; set; }
    }

    public class UnifiedPartItemDTO
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
    }
}
