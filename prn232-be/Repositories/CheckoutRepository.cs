using BusinessObjects.Common;
using DataAccessObjects;

namespace Repositories;

public class CheckoutRepository : ICheckoutRepository
{
    public ServiceResult CreateInvoice(CheckoutDto dto, int actingUserId, bool selfService = false)
        => CheckoutDAO.Instance.CreateInvoice(dto, actingUserId, selfService);
}
