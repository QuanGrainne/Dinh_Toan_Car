using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class CustomerCarDAO
    {
        private static CustomerCarDAO instance = null;
        private static readonly object instanceLock = new object();

        private CustomerCarDAO() { }

        public static CustomerCarDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CustomerCarDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<CustomerCar> GetCarsByCustomerId(int customerId)
        {
            using var context = new CarShowroomContext();
            return context.CustomerCars
                .Include(cc => cc.Brand)
                .Where(cc => cc.CustomerId == customerId)
                .ToList();
        }

        public CustomerCar GetById(int customerCarId)
        {
            using var context = new CarShowroomContext();
            return context.CustomerCars
                .Include(cc => cc.Brand)
                .SingleOrDefault(cc => cc.CustomerCarId == customerCarId);
        }

        public CustomerCar GetByLicensePlate(string licensePlate)
        {
            using var context = new CarShowroomContext();
            return context.CustomerCars
                .Include(cc => cc.Brand)
                .SingleOrDefault(cc => cc.LicensePlate == licensePlate);
        }

        public void AddCustomerCar(CustomerCar car)
        {
            using var context = new CarShowroomContext();
            context.CustomerCars.Add(car);
            context.SaveChanges();
        }

        public void UpdateCustomerCar(CustomerCar car)
        {
            using var context = new CarShowroomContext();
            context.Entry(car).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void DeleteCustomerCar(int customerCarId)
        {
            using var context = new CarShowroomContext();
            var car = context.CustomerCars.SingleOrDefault(cc => cc.CustomerCarId == customerCarId);
            if (car != null)
            {
                context.CustomerCars.Remove(car);
                context.SaveChanges();
            }
        }
    }
}
