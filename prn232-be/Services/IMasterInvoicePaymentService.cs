using System.Collections.Generic;
using BusinessObjects.Common;

namespace Services;

/// <summary>Thanh toán dùng chung trên MasterInvoice: sinh/xác thực captcha đặt cọc &amp; mua đứt cho mọi module.</summary>
public interface IMasterInvoicePaymentService
{
    ServiceResult GenerateDepositCaptcha(GenerateDepositCaptchaDto dto, int staffId);
    ServiceResult GenerateFinalCaptcha(int masterInvoiceId, int staffId);
    ServiceResult VerifyDeposit(VerifyCaptchaDto dto, int customerId);
    ServiceResult VerifyFinal(VerifyCaptchaDto dto, int customerId);
    int ReleaseExpiredDeposits();
    MasterInvoiceViewDto? GetInvoice(int masterInvoiceId, bool includeCaptcha);
    IEnumerable<MasterInvoiceViewDto> GetInvoices(int? customerId, string? invoiceType, bool includeCaptcha);
}
