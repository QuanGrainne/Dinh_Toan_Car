using System.Collections.Generic;
using BusinessObjects.Models;
using Repositories;

namespace Services
{
    public class PartCategoryService : IPartCategoryService
    {
        private readonly IPartCategoryRepository _repository;

        public PartCategoryService(IPartCategoryRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<PartCategory> GetAllCategories() => _repository.GetAllCategories();

        public PartCategory? GetCategoryById(int id) => _repository.GetCategoryById(id);

        public void AddCategory(PartCategory category) => _repository.AddCategory(category);

        public void UpdateCategory(PartCategory category) => _repository.UpdateCategory(category);

        public void DeleteCategory(int id) => _repository.DeleteCategory(id);
    }
}
