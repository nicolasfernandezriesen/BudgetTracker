using BudgetTracker.Models;
using BudgetTracker.ViewModels;

namespace BudgetTracker.Services.Income
{
    public interface IIncomeService
    {
        Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByUserAsync(int userId);
        Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByDateAsync(int userId, DateOnly date);
        Task<DetailBillIncomeViewModel> GetIncomeDetailsAsync(int userId, DateOnly date);
        Task<IncomeCreateViewModel> GetCreateViewModelAsync();
        Task CreateIncomeAsync(int userId, int amount, int categoryId, string desc, DateOnly date);
        Task UpdateIncomeAsync(int userId, int incomeId, int amount, int categoryId, string desc, DateOnly date);
        Task DeleteIncomeAsync(int userId, int incomeId, DateOnly date);
    }
}
