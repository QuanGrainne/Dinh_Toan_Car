using System.Collections.Generic;
using BusinessObjects.Common;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories;

public class CarSalesRepository : ICarSalesRepository
{
    public ServiceResult CreatePurchaseRequest(CreatePurchaseRequestDto dto, int customerId)
        => CarSalesDAO.Instance.CreatePurchaseRequest(dto, customerId);

    public IEnumerable<PurchaseRequest> GetPurchaseRequests(int? customerId)
        => CarSalesDAO.Instance.GetPurchaseRequests(customerId);
}
