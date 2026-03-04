using System.Linq.Expressions;

namespace BudgetTracker.Repositories.Category
{
    public interface ICategoryRepository : IRepository<BudgetTracker.Models.Category>
    {
        Task<BudgetTracker.Models.Category?> GetCategoryByNameAsync(string name);
    }
}
