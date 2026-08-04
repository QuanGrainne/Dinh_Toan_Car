using System.Collections.Generic;
using BusinessObjects.Models;
using BusinessObjects.Common;
using DataAccessObjects;

namespace Repositories;

public class CarRepository : ICarRepository
{
    public IEnumerable<Car> GetAllCars() => CarDAO.Instance.GetAllCars();

    public Car? GetCarById(int carId) => CarDAO.Instance.GetCarById(carId);

    public PagedResult<Car> GetPagedCars(CarSearchRequest request) => CarDAO.Instance.GetPagedCars(request);

    public void AddCar(Car car) => CarDAO.Instance.AddCar(car);

    public void UpdateCar(Car car) => CarDAO.Instance.UpdateCar(car);

    public void DeleteCar(int carId) => CarDAO.Instance.DeleteCar(carId);
}
