using BusinessObjects.Common;

namespace Services;

/// <summary>Tạo hóa đơn tổng (mua lẻ hoặc gộp nhiều module) rồi sinh mã captcha đặt cọc/mua đứt.</summary>
public interface ICheckoutService
{
    ServiceResult CreateInvoice(CheckoutDto dto, int actingUserId, bool selfService = false);
}
