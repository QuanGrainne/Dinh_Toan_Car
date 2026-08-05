using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public class CarDAO
{
    private static CarDAO instance = null;
    private static readonly object instanceLock = new object();

    private CarDAO() { }

    public static CarDAO Instance
    {
        get
        {
            lock (instanceLock)
            {
                if (instance == null)
                {
                    instance = new CarDAO();
                }
                return instance;
            }
        }
    }

    public IEnumerable<Car> GetAllCars()
    {
        using var context = new CarShowroomContext();
        return context.Cars.Include(c => c.Brand).ToList();
    }

    public Car? GetCarById(int carId)
    {
        using var context = new CarShowroomContext();
        return context.Cars.Include(c => c.Brand).SingleOrDefault(c => c.CarId == carId);
    }

    public PagedResult<Car> GetPagedCars(CarSearchRequest request)
    {
        using var context = new CarShowroomContext();
        IQueryable<Car> query = context.Cars.Include(c => c.Brand);

        // Filter out Inactive cars for showroom by default
        query = query.Where(c => c.Status != "Inactive");

        // Apply filters
        if (request.BrandId.HasValue)
        {
            query = query.Where(c => c.BrandId == request.BrandId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(c => c.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Transmission))
        {
            query = query.Where(c => c.Transmission == request.Transmission);
        }

        if (!string.IsNullOrWhiteSpace(request.FuelType))
        {
            query = query.Where(c => c.FuelType == request.FuelType);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower().Trim();
            query = query.Where(c => c.CarName.ToLower().Contains(term) || 
                                     (c.Model != null && c.Model.ToLower().Contains(term)));
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = request.SortBy.ToLower() switch
            {
                "priceasc" => query.OrderBy(c => c.Price),
                "pricedesc" => query.OrderByDescending(c => c.Price),
                "yeardesc" => query.OrderByDescending(c => c.Year),
                "mileageasc" => query.OrderBy(c => c.Mileage),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }

        // Get total count before paging
        int totalItems = query.Count();

        // Apply paging
        var items = query.Skip((request.PageNumber - 1) * request.PageSize)
                         .Take(request.PageSize)
                         .ToList();

        return new PagedResult<Car>(items, totalItems, request.PageNumber, request.PageSize);
    }

    public void AddCar(Car car)
    {
        using var context = new CarShowroomContext();
        context.Cars.Add(car);
        context.SaveChanges();
    }

    public void UpdateCar(Car car)
    {
        using var context = new CarShowroomContext();
        var existing = context.Cars.Find(car.CarId);
        if (existing == null) return;

        existing.BrandId = car.BrandId;
        existing.CarName = car.CarName;
        existing.Model = car.Model;
        existing.Year = car.Year;
        existing.Color = car.Color;
        existing.Mileage = car.Mileage;
        existing.FuelType = car.FuelType;
        existing.Transmission = car.Transmission;
        existing.Price = car.Price;
        existing.Description = car.Description;
        existing.ImageUrl = car.ImageUrl;
        existing.AdditionalImages = car.AdditionalImages;
        existing.ReviewUrl = car.ReviewUrl;
        existing.Status = car.Status;

        context.SaveChanges();
    }

    public void DeleteCar(int carId)
    {
        using var context = new CarShowroomContext();
        var car = context.Cars.SingleOrDefault(c => c.CarId == carId);
        if (car != null)
        {
            context.Cars.Remove(car);
            context.SaveChanges();
        }
    }
}
