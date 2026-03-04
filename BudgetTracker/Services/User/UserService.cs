using BudgetTracker.Models;
using BudgetTracker.Repositories.UserRepository;
using Microsoft.AspNetCore.Identity;

namespace BudgetTracker.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<BudgetTracker.Models.User> _passwordHasher;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher<BudgetTracker.Models.User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
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

        public async Task<Models.User?> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.UserPassword, password);
            if (verificationResult == PasswordVerificationResult.Success ||
                verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.UserPassword = _passwordHasher.HashPassword(user, password);
                    _userRepository.Update(user);
                    await _userRepository.SaveChangesAsync();
                }

                return user;
            }

            return null;
        }

        public async Task CreateUserAsync(Models.User user)
        {
            if (string.IsNullOrWhiteSpace(user.UserEmail))
            {
                throw new ArgumentException("Email cannot be empty.");
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(user.UserEmail);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            if (string.IsNullOrWhiteSpace(user.UserPassword))
            {
                throw new ArgumentException("Password cannot be empty.");
            }

            user.UserPassword = _passwordHasher.HashPassword(user, user.UserPassword);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(Models.User user)
        {
            if (user.UserId <= 0)
            {
                throw new ArgumentException("User ID must be valid.");
            }

            var existingUser = await _userRepository.GetByIdAsync(user.UserId);
            if (existingUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            existingUser.UserName = user.UserName;
            existingUser.UserEmail = user.UserEmail;

            if (!string.IsNullOrEmpty(user.UserPassword))
            {
                existingUser.UserPassword = _passwordHasher.HashPassword(existingUser, user.UserPassword);
            }

            _userRepository.Update(existingUser);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
        }
    }
}
