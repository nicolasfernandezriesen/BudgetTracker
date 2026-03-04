using System.Linq.Expressions;

namespace BudgetTracker.Repositories.User
{
    public interface IUserRepository : IRepository<BudgetTracker.Models.User>
    {
        Task<BudgetTracker.Models.User?> GetUserByEmailAsync(string email);
        Task<BudgetTracker.Models.User?> GetUserByNameAsync(string name);
    }
}
