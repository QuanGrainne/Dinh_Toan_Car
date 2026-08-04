using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class PartDAO
    {
        private static PartDAO instance = null;
        private static readonly object instanceLock = new object();

        private PartDAO() { }

        public static PartDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new PartDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<Part> GetAllParts()
        {
            using var context = new CarShowroomContext();
            return context.Parts.Include(p => p.Category).ToList();
        }

        public Part? GetPartById(int partId)
        {
            using var context = new CarShowroomContext();
            return context.Parts.Include(p => p.Category).SingleOrDefault(p => p.PartId == partId);
        }

        public void AddPart(Part part)
        {
            using var context = new CarShowroomContext();
            context.Parts.Add(part);
            context.SaveChanges();
        }

        public void UpdatePart(Part part)
        {
            using var context = new CarShowroomContext();
            context.Entry(part).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void DeletePart(int partId)
        {
            using var context = new CarShowroomContext();
            var part = context.Parts.SingleOrDefault(p => p.PartId == partId);
            if (part != null)
            {
                context.Parts.Remove(part);
                context.SaveChanges();
            }
        }

        public IEnumerable<Part> GetPartsFiltered(int categoryId, int supplierId)
        {
            using var context = new CarShowroomContext();
            var query = context.Parts.Include(p => p.Category).Where(p => p.CategoryId == categoryId);
            
            if (supplierId > 0)
            {
                query = query.Where(p => 
                    context.InventoryReceiptDetails.Any(d => d.PartId == p.PartId && d.Receipt.SupplierId == supplierId) ||
                    !context.InventoryReceiptDetails.Any(d => d.PartId == p.PartId)
                );
            }
            
            return query.ToList();
        }
    }
}
