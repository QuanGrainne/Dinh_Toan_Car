using System;

namespace BusinessObjects.DTOs
{
    public class AdminOrderListItemDto
    {
        public string OrderCode { get; set; } = string.Empty;

        // "Maintenance" hoặc "Part"
        public string OrderType { get; set; } = string.Empty;

        public int SourceId { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public decimal TotalAmount { get; set; }

        public string ProcessingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime? AppointmentDateTime { get; set; }

        public string? DeliveryMethod { get; set; }
    }
}
