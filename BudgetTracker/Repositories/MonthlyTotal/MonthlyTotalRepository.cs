using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Repositories.MonthlyTotal
{
    public class MonthlyTotalRepository : Repository<BudgetTracker.Models.MonthlyTotal>, IMonthlyTotalRepository
    {
        public MonthlyTotalRepository(BudgettrackerdbContext context) : base(context) { }

        public async Task<BudgetTracker.Models.MonthlyTotal?> GetMonthlyTotalByMonthAndUserAsync(int month, int year, int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(mt => 
                mt.MonthlyTotalsMonth == month && 
                mt.MonthlyTotalsYear == year && 
                mt.UserId == userId);
        }

        public async Task<IEnumerable<BudgetTracker.Models.MonthlyTotal>> GetMonthlyTotalsByUserAsync(int userId)
        {
            return await _dbSet
                .Where(mt => mt.UserId == userId)
                .ToListAsync();
        }
    }
}
