using System.Linq.Expressions;
using BudgetTracker.Models;

namespace BudgetTracker.Repositories.CategoryRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryByNameAsync(string name);
    }
}