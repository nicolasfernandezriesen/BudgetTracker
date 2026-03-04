using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Repositories.Category
{
    public class CategoryRepository : Repository<BudgetTracker.Models.Category>, ICategoryRepository
    {
        public CategoryRepository(BudgettrackerdbContext context) : base(context) { }

        public async Task<BudgetTracker.Models.Category?> GetCategoryByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CategoryName == name);
        }
    }
}
