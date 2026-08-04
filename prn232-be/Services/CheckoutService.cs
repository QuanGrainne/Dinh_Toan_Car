using BusinessObjects.Common;
using Repositories;

namespace Services;

public class CheckoutService : ICheckoutService
{
    private readonly ICheckoutRepository _repo;

    public CheckoutService(ICheckoutRepository repo)
    {
        _repo = repo;
    }

    public ServiceResult CreateInvoice(CheckoutDto dto, int actingUserId, bool selfService = false)
        => _repo.CreateInvoice(dto, actingUserId, selfService);
}
