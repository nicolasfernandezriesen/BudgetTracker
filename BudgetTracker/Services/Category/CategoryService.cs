using BudgetTracker.Models;
using BudgetTracker.Repositories.CategoryRepository;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<BudgetTracker.Models.Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<BudgetTracker.Models.Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        public async Task<BudgetTracker.Models.Category?> GetCategoryByNameAsync(string name)
        {
            return await _categoryRepository.GetCategoryByNameAsync(name);
        }

        public async Task<IEnumerable<SelectListItem>> GetCategoriesAsSelectListAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            });
        }
    }
}
