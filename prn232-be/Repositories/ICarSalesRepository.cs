using System.Collections.Generic;
using BusinessObjects.Common;
using BusinessObjects.Models;

namespace Repositories;

public interface ICarSalesRepository
{
    ServiceResult CreatePurchaseRequest(CreatePurchaseRequestDto dto, int customerId);
    IEnumerable<PurchaseRequest> GetPurchaseRequests(int? customerId);
}
