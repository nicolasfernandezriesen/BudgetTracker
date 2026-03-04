using BudgetTracker.Models;

namespace BudgetTracker.Repositories.UserRepository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByNameAsync(string name);
    }
}
