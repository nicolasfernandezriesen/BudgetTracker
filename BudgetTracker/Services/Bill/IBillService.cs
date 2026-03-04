using BudgetTracker.Models;
using BudgetTracker.ViewModels;

namespace BudgetTracker.Services.Bill
{
    public interface IBillService
    {
        Task<IEnumerable<BudgetTracker.Models.Bill>> GetBillsByUserAsync(int userId);
        Task<IEnumerable<BudgetTracker.Models.Bill>> GetBillsByDateAsync(int userId, DateOnly date);
        Task<DetailBillIncomeViewModel> GetBillDetailsAsync(int userId, DateOnly date);
        Task<BillViewModel> GetCreateViewModelAsync();
        Task<BillViewModel> GetEditViewModelAsync(int billId, int userId, DateOnly date);
        Task CreateBillAsync(int userId, int amount, int categoryId, string desc, DateOnly date);
        Task UpdateBillAsync(int userId, int billId, int amount, int categoryId, string desc, DateOnly date);
        Task DeleteBillAsync(int userId, int billId, DateOnly date);
    }
}
