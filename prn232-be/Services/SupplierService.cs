using System.Collections.Generic;
using BusinessObjects.Models;
using Repositories;

namespace Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;

        public SupplierService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Supplier> GetAllSuppliers() => _repository.GetAllSuppliers();

        public Supplier? GetSupplierById(int id) => _repository.GetSupplierById(id);

        public void AddSupplier(Supplier supplier) => _repository.AddSupplier(supplier);

        public void UpdateSupplier(Supplier supplier) => _repository.UpdateSupplier(supplier);

        public void DeleteSupplier(int id) => _repository.DeleteSupplier(id);
    }
}
