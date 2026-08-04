using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class CustomerCarRepository : ICustomerCarRepository
    {
        public IEnumerable<CustomerCar> GetCarsByCustomerId(int customerId) => CustomerCarDAO.Instance.GetCarsByCustomerId(customerId);
        public CustomerCar GetById(int customerCarId) => CustomerCarDAO.Instance.GetById(customerCarId);
        public CustomerCar GetByLicensePlate(string licensePlate) => CustomerCarDAO.Instance.GetByLicensePlate(licensePlate);
        public void AddCustomerCar(CustomerCar car) => CustomerCarDAO.Instance.AddCustomerCar(car);
        public void UpdateCustomerCar(CustomerCar car) => CustomerCarDAO.Instance.UpdateCustomerCar(car);
        public void DeleteCustomerCar(int customerCarId) => CustomerCarDAO.Instance.DeleteCustomerCar(customerCarId);
    }
}
