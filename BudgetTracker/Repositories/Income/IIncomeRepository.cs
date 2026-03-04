using System.Linq.Expressions;
using BudgetTracker.Models;

namespace BudgetTracker.Repositories.IncomeRepository
{
    public interface IIncomeRepository : IRepository<Income>
    {
        Task<IEnumerable<Income>> GetIncomeByUserIdAsync(int userId);
        Task<IEnumerable<Income>> GetIncomeByUserAndDateAsync(int userId, DateOnly date);
        Task<IEnumerable<Income>> GetIncomeByMonthAsync(int userId, int month, int year);
        Task<Income?> GetIncomeByIdAndUserAsync(int incomeId, int userId);
    }
}
