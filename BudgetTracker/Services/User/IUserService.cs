using BudgetTracker.Models;
using BudgetTracker.ViewModels.User;

namespace BudgetTracker.Services.User
{
    public interface IUserService
    {
        Task<BudgetTracker.Models.User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<BudgetTracker.Models.User>> GetAllUsersAsync();
        Task<BudgetTracker.Models.User?> GetUserByEmailAsync(string email);
        Task<BudgetTracker.Models.User?> GetUserByNameAsync(string name);
        Task UpdateUserAsync(EditViewModel user, int userId);
        Task ResetPasswordAsync(ResetPasswordViewModel model);
        Task DeleteUserAsync(int userId);
    }
}
