using System.Collections.Generic;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IPartCategoryRepository
    {
        IEnumerable<PartCategory> GetAllCategories();
        PartCategory? GetCategoryById(int id);
        void AddCategory(PartCategory category);
        void UpdateCategory(PartCategory category);
        void DeleteCategory(int id);
    }
}
