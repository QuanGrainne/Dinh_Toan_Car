using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class PartCategoryDAO
    {
        private static PartCategoryDAO instance = null;
        private static readonly object instanceLock = new object();

        private PartCategoryDAO() { }

        public static PartCategoryDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new PartCategoryDAO();
                    }
                    return instance;
                }
            }
        }

        public IEnumerable<PartCategory> GetAllCategories()
        {
            using var context = new CarShowroomContext();
            return context.PartCategories.ToList();
        }

        public PartCategory? GetCategoryById(int id)
        {
            using var context = new CarShowroomContext();
            return context.PartCategories.SingleOrDefault(c => c.CategoryId == id);
        }

        public void AddCategory(PartCategory category)
        {
            using var context = new CarShowroomContext();
            category.CreatedAt = DateTime.Now;
            context.PartCategories.Add(category);
            context.SaveChanges();
        }

        public void UpdateCategory(PartCategory category)
        {
            using var context = new CarShowroomContext();
            var existing = context.PartCategories.SingleOrDefault(c => c.CategoryId == category.CategoryId);
            if (existing != null)
            {
                existing.CategoryName = category.CategoryName;
                existing.Description = category.Description;
                existing.UpdatedAt = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void DeleteCategory(int id)
        {
            using var context = new CarShowroomContext();
            var category = context.PartCategories.SingleOrDefault(c => c.CategoryId == id);
            if (category != null)
            {
                context.PartCategories.Remove(category);
                context.SaveChanges();
            }
        }
    }
}
