using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.ViewModels
{
    public sealed class NewPartViewModel
    {
        [Required(ErrorMessage = "CategoryId là bắt buộc")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên phụ tùng là bắt buộc")]
        public string PartName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã phụ tùng là bắt buộc")]
        public string PartCode { get; set; } = string.Empty;

        public string? Brand { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá bán không được âm")]
        public decimal Price { get; set; }

        [Range(0, 100000, ErrorMessage = "Định mức tối thiểu không được âm")]
        public int MinStockLevel { get; set; } = 5;

        [Range(1, 100000, ErrorMessage = "Định mức tối đa phải lớn hơn 0")]
        public int MaxStockLevel { get; set; } = 100;

        [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
        public string UnitOfMeasure { get; set; } = "Cái";

        public string? WarehouseLocation { get; set; }

        [Range(0, 1000, ErrorMessage = "Số tháng bảo hành không được âm")]
        public int WarrantyMonths { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class InventoryReceiptItemViewModel
    {
        public bool IsNewPart { get; set; }

        public int? SupplierId { get; set; }

        // Dùng khi chọn phụ tùng có sẵn
        public int? PartId { get; set; }

        [Range(1, 100000, ErrorMessage = "Số lượng nhập phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá nhập không được âm")]
        public decimal ImportPrice { get; set; }

        public DateTime? ExpiredAt { get; set; }

        // Chỉ dùng khi IsNewPart = true
        public NewPartViewModel? NewPart { get; set; }
    }

    public class InventoryReceiptCreateViewModel
    {
        public int? SupplierId { get; set; }

        public string? Notes { get; set; }

        // Chỉ dùng lọc UI, không lưu database
        public int? CategoryFilterId { get; set; }

        public List<InventoryReceiptItemViewModel> Items { get; set; } = new List<InventoryReceiptItemViewModel>();
    }
}
