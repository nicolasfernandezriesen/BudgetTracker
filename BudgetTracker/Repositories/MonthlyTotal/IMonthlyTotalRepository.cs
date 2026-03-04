using System.Linq.Expressions;
using BudgetTracker.Models;

namespace BudgetTracker.Repositories.MonthlyTotalRepository
{
    public interface IMonthlyTotalRepository : IRepository<MonthlyTotal>
    {
        Task<MonthlyTotal?> GetMonthlyTotalByMonthAndUserAsync(int month, int year, int userId);
        Task<IEnumerable<MonthlyTotal>> GetMonthlyTotalsByUserAsync(int userId);
    }
}
