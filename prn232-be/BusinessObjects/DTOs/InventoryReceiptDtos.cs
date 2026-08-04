using System;
using System.Collections.Generic;

namespace BusinessObjects.DTOs
{
    public class NewPartDto
    {
        public int CategoryId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public int MinStockLevel { get; set; } = 5;
        public int MaxStockLevel { get; set; } = 100;
        public string UnitOfMeasure { get; set; } = "Cái";
        public string? WarehouseLocation { get; set; }
        public int WarrantyMonths { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class InventoryReceiptItemDto
    {
        public bool IsNewPart { get; set; }
        public int? PartId { get; set; }
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public NewPartDto? NewPart { get; set; }
    }

    public class InventoryReceiptCreateDto
    {
        public int SupplierId { get; set; }
        public string? Notes { get; set; }
        public List<InventoryReceiptItemDto> Items { get; set; } = new List<InventoryReceiptItemDto>();
    }

    public class InventoryReceiptResponseDto
    {
        public int ReceiptId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Success";
        public string Message { get; set; } = null!;
    }
}
