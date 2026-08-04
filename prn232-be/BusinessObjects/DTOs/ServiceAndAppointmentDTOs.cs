using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
    // DTO đơn giản cho Service
    public class ServiceDTO
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Ten dich vu khong duoc trong")]
        [StringLength(150)]
        public string ServiceName { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, 1000000000)]
        public decimal BasePrice { get; set; }

        [Range(1, 10000)]
        public int EstimatedDurationMinutes { get; set; }

        public string Status { get; set; } = "Available";
        public DateTime? CreatedAt { get; set; }
    }

    // DTO tóm tắt service (dùng trong danh sách của package)
    public class ServiceSummaryDTO
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public int EstimatedDurationMinutes { get; set; }
    }

    // DTO cho CustomerCar
    public class CustomerCarDTO
    {
        public int CustomerCarId { get; set; }
        public int CustomerId { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = null!;

        public int? Year { get; set; }

        [StringLength(50)]
        public string? VIN { get; set; }

        [Required]
        [StringLength(30)]
        public string LicensePlate { get; set; } = null!;

        [StringLength(50)]
        public string? Color { get; set; }

        public DateTime? ExpiredAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    // DTO chi tiết lịch hẹn (từng dòng gói/dịch vụ)
    public class AppointmentDetailDTO
    {
        public int AppointmentDetailId { get; set; }
        public int? PackageId { get; set; }
        public string? PackageName { get; set; }
        public int? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;

        // Danh sách dịch vụ nếu là gói
        public List<ServiceSummaryDTO> PackageServices { get; set; } = new();
    }

    // DTO phụ tùng tiêu thụ
    public class ConsumedPartDTO
    {
        public int ConsumedPartId { get; set; }
        public int PartId { get; set; }
        public string? PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
        public bool IsIncurred { get; set; }
        public bool ApprovedByCustomer { get; set; }
        public string? Notes { get; set; }
    }

    // DTO kỹ thuật viên báo cáo phụ tùng phát sinh
    public class IncurredPartReportDto
    {
        [Required]
        public int AppointmentId { get; set; }
        public int? AppointmentDetailId { get; set; }
        [Required]
        public int PartId { get; set; }
        [Range(1, 1000)]
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }

    // DTO khách hàng phê duyệt/từ chối phụ tùng phát sinh
    public class IncurredPartApprovalDto
    {
        [Required]
        public int ConsumedPartId { get; set; }
        public bool IsApproved { get; set; }
    }
}
