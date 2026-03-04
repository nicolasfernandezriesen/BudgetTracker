using System.Linq.Expressions;

namespace BudgetTracker.Repositories.Income
{
    public interface IIncomeRepository : IRepository<BudgetTracker.Models.Income>
    {
        Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByUserIdAsync(int userId);
        Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByUserAndDateAsync(int userId, DateOnly date);
        Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByMonthAsync(int userId, int month, int year);
        Task<BudgetTracker.Models.Income?> GetIncomeByIdAndUserAsync(int incomeId, int userId);
    }
}
