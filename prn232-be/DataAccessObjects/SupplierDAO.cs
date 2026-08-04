using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class SupplierDAO
    {
        private static SupplierDAO? instance = null;
        private static readonly object instanceLock = new object();

        private SupplierDAO() { }

        public static SupplierDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new SupplierDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<Supplier> GetAllSuppliers()
        {
            using var context = new CarShowroomContext();
            return context.Suppliers.ToList();
        }

        public Supplier? GetSupplierById(int id)
        {
            using var context = new CarShowroomContext();
            return context.Suppliers.SingleOrDefault(s => s.SupplierId == id);
        }

        public void AddSupplier(Supplier supplier)
        {
            using var context = new CarShowroomContext();
            context.Suppliers.Add(supplier);
            context.SaveChanges();
        }

        public void UpdateSupplier(Supplier supplier)
        {
            using var context = new CarShowroomContext();
            var existing = context.Suppliers.SingleOrDefault(s => s.SupplierId == supplier.SupplierId);
            if (existing != null)
            {
                existing.SupplierName = supplier.SupplierName;
                existing.ContactName = supplier.ContactName;
                existing.Phone = supplier.Phone;
                existing.Email = supplier.Email;
                existing.Address = supplier.Address;
                existing.Status = supplier.Status;
                existing.UpdatedAt = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void DeleteSupplier(int id)
        {
            using var context = new CarShowroomContext();
            var supplier = context.Suppliers.SingleOrDefault(s => s.SupplierId == id);
            if (supplier != null)
            {
                context.Suppliers.Remove(supplier);
                context.SaveChanges();
            }
        }
    }
}
