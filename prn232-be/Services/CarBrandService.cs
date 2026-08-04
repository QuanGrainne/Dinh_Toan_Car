using System.Collections.Generic;
using BusinessObjects.Models;
using Repositories;

namespace Services;

public class CarBrandService : ICarBrandService
{
    private readonly ICarBrandRepository _carBrandRepository;

    public CarBrandService(ICarBrandRepository carBrandRepository)
    {
        _carBrandRepository = carBrandRepository;
    }

    public IEnumerable<CarBrand> GetAllBrands() => _carBrandRepository.GetAllBrands();

    public CarBrand? GetBrandById(int brandId) => _carBrandRepository.GetBrandById(brandId);
}
