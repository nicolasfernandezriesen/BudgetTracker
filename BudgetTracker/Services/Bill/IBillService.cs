using BillModel = BudgetTracker.Models.Bill;
using BudgetTracker.ViewModels;
using BudgetTracker.ViewModels.Bill;

namespace BudgetTracker.Services.Bill
{
    public interface IBillService
    {
        Task<IEnumerable<BillModel>> GetBillsByUserAsync(int userId);
        Task<IEnumerable<BillModel>> GetBillsByDateAsync(int userId, DateOnly date);
        Task<DetailBillIncomeViewModel> GetBillDetailsAsync(int userId, DateOnly date);
        Task<CreateViewModel> GetCreateViewModelAsync();
        Task<EditViewModel> GetEditViewModelAsync(int billId, int userId, DateOnly date);
        Task CreateBillAsync(int userId, CreateViewModel model);
        Task UpdateBillAsync(int userId, EditViewModel model);
        Task DeleteBillAsync(int userId, int billId, DateOnly date);
    }
}
