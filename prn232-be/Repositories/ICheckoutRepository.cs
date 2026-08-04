using BusinessObjects.Common;

namespace Repositories;

public interface ICheckoutRepository
{
    ServiceResult CreateInvoice(CheckoutDto dto, int actingUserId, bool selfService = false);
}
