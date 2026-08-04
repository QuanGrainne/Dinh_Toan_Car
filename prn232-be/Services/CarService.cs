using System.Collections.Generic;
using BusinessObjects.Models;
using BusinessObjects.Common;
using Repositories;

namespace Services;

public class CarService : ICarService
{
    private readonly ICarRepository _carRepository;

    public CarService(ICarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    public IEnumerable<Car> GetAllCars() => _carRepository.GetAllCars();

    public Car? GetCarById(int carId) => _carRepository.GetCarById(carId);

    public PagedResult<Car> GetPagedCars(CarSearchRequest request) => _carRepository.GetPagedCars(request);

    public void AddCar(Car car) => _carRepository.AddCar(car);

    public void UpdateCar(Car car) => _carRepository.UpdateCar(car);

    public void DeleteCar(int carId) => _carRepository.DeleteCar(carId);
}
