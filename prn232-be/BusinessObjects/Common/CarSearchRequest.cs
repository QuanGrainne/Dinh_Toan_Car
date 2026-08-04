using System;

namespace BusinessObjects.Common;

public class CarSearchRequest : PagingRequest
{
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Transmission { get; set; } // "Manual", "Automatic"
    public string? FuelType { get; set; }       // "Gasoline", "Diesel", "Electric", "Hybrid"
    public string? SearchTerm { get; set; }     // Search by CarName or Model
    public string? SortBy { get; set; }         // "PriceAsc", "PriceDesc", "YearDesc", "MileageAsc"
}
