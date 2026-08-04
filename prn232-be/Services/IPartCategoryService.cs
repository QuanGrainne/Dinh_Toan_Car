using System.Collections.Generic;
using BusinessObjects.Models;

namespace Services
{
    public interface IPartCategoryService
    {
        IEnumerable<PartCategory> GetAllCategories();
        PartCategory? GetCategoryById(int id);
        void AddCategory(PartCategory category);
        void UpdateCategory(PartCategory category);
        void DeleteCategory(int id);
    }
}
