using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.ViewModels
{
    public sealed class UpdatePartViewModel
    {
        public int PartId { get; set; }

        [Required(ErrorMessage = "Tên phụ tùng không được để trống.")]
        [StringLength(150, ErrorMessage = "Tên phụ tùng không vượt quá 150 ký tự.")]
        public string PartName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã phụ tùng không được để trống.")]
        [StringLength(50, ErrorMessage = "Mã phụ tùng không vượt quá 50 ký tự.")]
        public string PartCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục phụ tùng.")]
        [Range(1, int.MaxValue, ErrorMessage = "Danh mục không hợp lệ.")]
        public int CategoryId { get; set; }

        [StringLength(100, ErrorMessage = "Thương hiệu không vượt quá 100 ký tự.")]
        public string? Brand { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá bán không được âm.")]
        public decimal Price { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không vượt quá 1000 ký tự.")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "Đường dẫn hình ảnh không vượt quá 500 ký tự.")]
        public string? ImageUrl { get; set; }

        public string Status { get; set; } = "Available";

        public bool CanEditPartCode { get; set; } = true;
    }

    public sealed class InventoryAdjustmentViewModel
    {
        [Required(ErrorMessage = "Mã ID phụ tùng là bắt buộc.")]
        public int PartId { get; set; }

        [Required(ErrorMessage = "Số lượng điều chỉnh không được bằng 0.")]
        public int AdjustmentQuantity { get; set; }

        [Required(ErrorMessage = "Lý do điều chỉnh không được để trống.")]
        [StringLength(200, ErrorMessage = "Lý do điều chỉnh không vượt quá 200 ký tự.")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không vượt quá 500 ký tự.")]
        public string? Notes { get; set; }
    }
}
