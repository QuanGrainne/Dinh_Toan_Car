using System.Collections.Generic;
using BusinessObjects.Common;
using BusinessObjects.Models;

namespace Services;

/// <summary>Yêu cầu mua xe của khách hàng. Thanh toán/đặt cọc dùng tầng chung (Checkout + Invoices).</summary>
public interface ICarSalesService
{
    ServiceResult CreatePurchaseRequest(CreatePurchaseRequestDto dto, int customerId);
    IEnumerable<PurchaseRequest> GetPurchaseRequests(int? customerId);
}
