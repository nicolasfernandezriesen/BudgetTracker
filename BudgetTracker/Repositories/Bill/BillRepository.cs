using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Repositories.BillRepository
{
    public class BillRepository : Repository<Bill>, IBillRepository
    {
        public BillRepository(BudgettrackerdbContext context) : base(context) { }

        public async Task<IEnumerable<Bill>> GetBillsByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Bill>> GetBillsByUserAndDateAsync(int userId, DateOnly date)
        {
            return await _dbSet
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.BillsDate == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Bill>> GetBillsByMonthAsync(int userId, int month, int year)
        {
            return await _dbSet
                .Where(b => b.UserId == userId && b.BillsDate.Month == month && b.BillsDate.Year == year)
                .ToListAsync();
        }

        public async Task<Bill?> GetBillByIdAndUserAsync(int billId, int userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(b => b.BillsId == billId && b.UserId == userId);
        }
    }
}