using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface ICustomerCarRepository
    {
        IEnumerable<CustomerCar> GetCarsByCustomerId(int customerId);
        CustomerCar GetById(int customerCarId);
        CustomerCar GetByLicensePlate(string licensePlate);
        void AddCustomerCar(CustomerCar car);
        void UpdateCustomerCar(CustomerCar car);
        void DeleteCustomerCar(int customerCarId);
    }
}
