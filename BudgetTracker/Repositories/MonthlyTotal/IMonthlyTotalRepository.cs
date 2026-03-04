using System.Linq.Expressions;

namespace BudgetTracker.Repositories.MonthlyTotal
{
    public interface IMonthlyTotalRepository : IRepository<BudgetTracker.Models.MonthlyTotal>
    {
        Task<BudgetTracker.Models.MonthlyTotal?> GetMonthlyTotalByMonthAndUserAsync(int month, int year, int userId);
        Task<IEnumerable<BudgetTracker.Models.MonthlyTotal>> GetMonthlyTotalsByUserAsync(int userId);
    }
}
