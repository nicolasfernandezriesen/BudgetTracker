using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Repositories.Income
{
    public class IncomeRepository : Repository<BudgetTracker.Models.Income>, IIncomeRepository
    {
        public IncomeRepository(BudgettrackerdbContext context) : base(context) { }

        public async Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(i => i.Category)
                .Where(i => i.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByUserAndDateAsync(int userId, DateOnly date)
        {
            return await _dbSet
                .Include(i => i.Category)
                .Where(i => i.UserId == userId && i.IncomeDate == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByMonthAsync(int userId, int month, int year)
        {
            return await _dbSet
                .Where(i => i.UserId == userId && i.IncomeDate.Month == month && i.IncomeDate.Year == year)
                .ToListAsync();
        }

        public async Task<BudgetTracker.Models.Income?> GetIncomeByIdAndUserAsync(int incomeId, int userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.IncomeId == incomeId && i.UserId == userId);
        }
    }
}
