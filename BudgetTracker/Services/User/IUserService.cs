using BudgetTracker.Models;

namespace BudgetTracker.Services.User
{
    public interface IUserService
    {
        Task<BudgetTracker.Models.User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<BudgetTracker.Models.User>> GetAllUsersAsync();
        Task<BudgetTracker.Models.User?> GetUserByEmailAsync(string email);
        Task<BudgetTracker.Models.User?> GetUserByNameAsync(string name);
        Task<BudgetTracker.Models.User?> ValidateCredentialsAsync(string email, string password);
        Task CreateUserAsync(BudgetTracker.Models.User user);
        Task UpdateUserAsync(BudgetTracker.Models.User user);
        Task DeleteUserAsync(int userId);
    }
}
