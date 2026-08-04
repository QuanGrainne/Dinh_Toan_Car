using System.Collections.Generic;
using BusinessObjects.Common;
using BusinessObjects.Models;
using Repositories;

namespace Services;

/// <summary>Nghiệp vụ yêu cầu mua xe (tạo &amp; tra cứu). Phần hóa đơn/đặt cọc/mua đứt nằm ở tầng dùng chung.</summary>
public class CarSalesService : ICarSalesService
{
    private readonly ICarSalesRepository _repo;

    public CarSalesService(ICarSalesRepository repo)
    {
        _repo = repo;
    }

    public ServiceResult CreatePurchaseRequest(CreatePurchaseRequestDto dto, int customerId)
        => _repo.CreatePurchaseRequest(dto, customerId);

    public IEnumerable<PurchaseRequest> GetPurchaseRequests(int? customerId)
        => _repo.GetPurchaseRequests(customerId);
}
