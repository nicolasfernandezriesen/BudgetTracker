using System.Linq.Expressions;

namespace BudgetTracker.Repositories.Bill
{
    public interface IBillRepository : IRepository<BudgetTracker.Models.Bill>
    {
        Task<IEnumerable<BudgetTracker.Models.Bill>> GetBillsByUserIdAsync(int userId);
        Task<IEnumerable<BudgetTracker.Models.Bill>> GetBillsByUserAndDateAsync(int userId, DateOnly date);
        Task<IEnumerable<BudgetTracker.Models.Bill>> GetBillsByMonthAsync(int userId, int month, int year);
        Task<BudgetTracker.Models.Bill?> GetBillByIdAndUserAsync(int billId, int userId);
    }
}
