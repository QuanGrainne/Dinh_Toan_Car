using System.Collections.Generic;
using BusinessObjects.Common;

namespace Repositories;

public interface IMasterInvoicePaymentRepository
{
    ServiceResult GenerateDepositCaptcha(int masterInvoiceId, decimal? depositAmount, int expiresInDays, int staffId);
    ServiceResult GenerateFinalCaptcha(int masterInvoiceId, int staffId);
    ServiceResult VerifyDeposit(int masterInvoiceId, string code, int customerId);
    ServiceResult VerifyFinal(int masterInvoiceId, string code, int customerId);
    int ReleaseExpiredDeposits();
    MasterInvoiceViewDto? GetInvoice(int masterInvoiceId, bool includeCaptcha);
    IEnumerable<MasterInvoiceViewDto> GetInvoices(int? customerId, string? invoiceType, bool includeCaptcha);
}
