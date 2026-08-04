using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;

namespace DataAccessObjects;

public class CarBrandDAO
{
    private static CarBrandDAO instance = null;
    private static readonly object instanceLock = new object();

    private CarBrandDAO() { }

    public static CarBrandDAO Instance
    {
        get
        {
            lock (instanceLock)
            {
                if (instance == null)
                {
                    instance = new CarBrandDAO();
                }
                return instance;
            }
        }
    }

    public IEnumerable<CarBrand> GetAllBrands()
    {
        using var context = new CarShowroomContext();
        return context.CarBrands.ToList();
    }

    public CarBrand? GetBrandById(int brandId)
    {
        using var context = new CarShowroomContext();
        return context.CarBrands.SingleOrDefault(b => b.BrandId == brandId);
    }
}
