using System.Collections.Generic;
using BusinessObjects.Models;

namespace Services;

public interface ICarBrandService
{
    IEnumerable<CarBrand> GetAllBrands();
    CarBrand? GetBrandById(int brandId);
}
