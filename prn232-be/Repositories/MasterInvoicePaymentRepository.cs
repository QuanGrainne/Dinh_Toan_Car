using System.Collections.Generic;
using BusinessObjects.Common;
using DataAccessObjects;

namespace Repositories;

public class MasterInvoicePaymentRepository : IMasterInvoicePaymentRepository
{
    public ServiceResult GenerateDepositCaptcha(int masterInvoiceId, decimal? depositAmount, int expiresInDays, int staffId)
        => MasterInvoicePaymentDAO.Instance.GenerateDepositCaptcha(masterInvoiceId, depositAmount, expiresInDays, staffId);

    public ServiceResult GenerateFinalCaptcha(int masterInvoiceId, int staffId)
        => MasterInvoicePaymentDAO.Instance.GenerateFinalCaptcha(masterInvoiceId, staffId);

    public ServiceResult VerifyDeposit(int masterInvoiceId, string code, int customerId)
        => MasterInvoicePaymentDAO.Instance.VerifyDeposit(masterInvoiceId, code, customerId);

    public ServiceResult VerifyFinal(int masterInvoiceId, string code, int customerId)
        => MasterInvoicePaymentDAO.Instance.VerifyFinal(masterInvoiceId, code, customerId);

    public int ReleaseExpiredDeposits()
        => MasterInvoicePaymentDAO.Instance.ReleaseExpiredDeposits();

    public MasterInvoiceViewDto? GetInvoice(int masterInvoiceId, bool includeCaptcha)
        => MasterInvoicePaymentDAO.Instance.GetInvoice(masterInvoiceId, includeCaptcha);

    public IEnumerable<MasterInvoiceViewDto> GetInvoices(int? customerId, string? invoiceType, bool includeCaptcha)
        => MasterInvoicePaymentDAO.Instance.GetInvoices(customerId, invoiceType, includeCaptcha);
}
