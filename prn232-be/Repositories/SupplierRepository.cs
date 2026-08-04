using System.Collections.Generic;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        public IEnumerable<Supplier> GetAllSuppliers() => SupplierDAO.Instance.GetAllSuppliers();

        public Supplier? GetSupplierById(int id) => SupplierDAO.Instance.GetSupplierById(id);

        public void AddSupplier(Supplier supplier) => SupplierDAO.Instance.AddSupplier(supplier);

        public void UpdateSupplier(Supplier supplier) => SupplierDAO.Instance.UpdateSupplier(supplier);

        public void DeleteSupplier(int id) => SupplierDAO.Instance.DeleteSupplier(id);
    }
}
