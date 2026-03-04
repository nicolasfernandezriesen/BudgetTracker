using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.Services.Category
{
    public interface ICategoryService
    {
        Task<IEnumerable<BudgetTracker.Models.Category>> GetAllCategoriesAsync();
        Task<BudgetTracker.Models.Category?> GetCategoryByIdAsync(int categoryId);
        Task<BudgetTracker.Models.Category?> GetCategoryByNameAsync(string name);
        Task<IEnumerable<SelectListItem>> GetCategoriesAsSelectListAsync();
    }
}
