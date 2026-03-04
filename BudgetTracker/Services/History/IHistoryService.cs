using BudgetTracker.ViewModels;

namespace BudgetTracker.Services.History
{
    public interface IHistoryService
    {
        Task<HistoryViewModel> GetBillIncomeAndMonthlyTotalAsync(int userId, int month, int year);
    }
}
