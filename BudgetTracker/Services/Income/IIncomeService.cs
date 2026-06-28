using IncomeModel = BudgetTracker.Models.Income;
using BudgetTracker.ViewModels;
using BudgetTracker.ViewModels.Income;

namespace BudgetTracker.Services.Income
{
    public interface IIncomeService
    {
        Task<IEnumerable<IncomeModel>> GetIncomeByUserAsync(int userId);
        Task<IEnumerable<IncomeModel>> GetIncomeByDateAsync(int userId, DateOnly date);
        Task<DetailBillIncomeViewModel> GetIncomeDetailsAsync(int userId, DateOnly date);
        Task<CreateViewModel> GetCreateViewModelAsync();
        Task CreateIncomeAsync(int userId, CreateViewModel viewModel);
        Task<EditViewModel> GetEditViewModelAsync(int userId, int incomeId);
        Task UpdateIncomeAsync(int userId, int incomeId, EditViewModel viewModel);
        Task DeleteIncomeAsync(int userId, int incomeId, DateOnly date);
    }
}
