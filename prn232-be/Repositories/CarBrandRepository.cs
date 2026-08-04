using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories;

public class CarBrandRepository : ICarBrandRepository
{
    public IEnumerable<CarBrand> GetAllBrands() => CarBrandDAO.Instance.GetAllBrands();

    public CarBrand? GetBrandById(int brandId) => CarBrandDAO.Instance.GetBrandById(brandId);
}
