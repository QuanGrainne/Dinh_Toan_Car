namespace BusinessObjects.DTOs
{
    /// <summary>
    /// Một mục gợi ý do chatbot tư vấn đề xuất (xe/phụ tùng/dịch vụ).
    /// Chatbot chỉ tư vấn, KHÔNG tạo đơn hàng — việc chốt đơn xe đi qua luồng
    /// PurchaseRequest + MasterInvoice (xác thực bằng mã captcha do nhân viên tạo).
    /// </summary>
    public class ComboOrderItemDto
    {
        public int ReferenceId { get; set; }
        public string ItemType { get; set; } = null!;
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
