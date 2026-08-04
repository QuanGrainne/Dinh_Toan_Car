using System.Collections.Generic;
using BusinessObjects.Models;
using BusinessObjects.Common;

namespace Repositories;

public interface ICarRepository
{
    IEnumerable<Car> GetAllCars();
    Car? GetCarById(int carId);
    PagedResult<Car> GetPagedCars(CarSearchRequest request);
    void AddCar(Car car);
    void UpdateCar(Car car);
    void DeleteCar(int carId);
}
