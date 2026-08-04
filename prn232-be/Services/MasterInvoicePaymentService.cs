using System.Collections.Generic;
using BusinessObjects.Common;
using Repositories;

namespace Services;

public class MasterInvoicePaymentService : IMasterInvoicePaymentService
{
    private readonly IMasterInvoicePaymentRepository _repo;

    public MasterInvoicePaymentService(IMasterInvoicePaymentRepository repo)
    {
        _repo = repo;
    }

    public ServiceResult GenerateDepositCaptcha(GenerateDepositCaptchaDto dto, int staffId)
        => _repo.GenerateDepositCaptcha(dto.MasterInvoiceId, dto.DepositAmount, dto.DepositExpiresInDays, staffId);

    public ServiceResult GenerateFinalCaptcha(int masterInvoiceId, int staffId)
        => _repo.GenerateFinalCaptcha(masterInvoiceId, staffId);

    public ServiceResult VerifyDeposit(VerifyCaptchaDto dto, int customerId)
        => _repo.VerifyDeposit(dto.MasterInvoiceId, dto.CaptchaCode, customerId);

    public ServiceResult VerifyFinal(VerifyCaptchaDto dto, int customerId)
        => _repo.VerifyFinal(dto.MasterInvoiceId, dto.CaptchaCode, customerId);

    public int ReleaseExpiredDeposits()
        => _repo.ReleaseExpiredDeposits();

    public MasterInvoiceViewDto? GetInvoice(int masterInvoiceId, bool includeCaptcha)
        => _repo.GetInvoice(masterInvoiceId, includeCaptcha);

    public IEnumerable<MasterInvoiceViewDto> GetInvoices(int? customerId, string? invoiceType, bool includeCaptcha)
        => _repo.GetInvoices(customerId, invoiceType, includeCaptcha);
}
