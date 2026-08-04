using System;

namespace BusinessObjects.DTOs
{
    public class CustomerOrderDto
    {
        public string OrderCode { get; set; } = string.Empty;

        // Maintenance hoặc Part
        public string OrderType { get; set; } = string.Empty;

        public int SourceId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Summary { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public bool IsEstimatedAmount { get; set; }

        public string ProcessingStatus { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime? AppointmentDateTime { get; set; }

        public string? DeliveryMethod { get; set; }
    }
}
