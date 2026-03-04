using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Repositories.CategoryRepository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(BudgettrackerdbContext context) : base(context) { }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CategoryName == name);
        }
    }
}
