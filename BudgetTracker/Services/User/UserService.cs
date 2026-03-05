using BudgetTracker.Models;
using BudgetTracker.Repositories.UserRepository;
using Microsoft.AspNetCore.Identity;

namespace BudgetTracker.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<Models.User> _userManager;

        public UserService(
            IUserRepository userRepository,
            UserManager<Models.User> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<Models.User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<IEnumerable<Models.User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<Models.User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<Models.User?> GetUserByNameAsync(string name)
        {
            return await _userRepository.GetUserByNameAsync(name);
        }

        public async Task UpdateUserAsync(Models.User user)
        {
            if (user.Id <= 0)
            {
                throw new ArgumentException("User ID must be valid.");
            }

            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            if (existingUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to delete user: {errors}");
            }
        }
    }
}
