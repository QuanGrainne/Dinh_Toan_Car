using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories;

public interface ICarBrandRepository
{
    IEnumerable<CarBrand> GetAllBrands();
    CarBrand? GetBrandById(int brandId);
}
