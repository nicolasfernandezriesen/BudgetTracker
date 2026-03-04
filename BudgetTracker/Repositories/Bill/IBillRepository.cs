using BudgetTracker.Models;

namespace BudgetTracker.Repositories.BillRepository
{
    public interface IBillRepository : IRepository<Bill>
    {
        Task<IEnumerable<Bill>> GetBillsByUserIdAsync(int userId);
        Task<IEnumerable<Bill>> GetBillsByUserAndDateAsync(int userId, DateOnly date);
        Task<IEnumerable<Bill>> GetBillsByMonthAsync(int userId, int month, int year);
        Task<Bill?> GetBillByIdAndUserAsync(int billId, int userId);
    }
}