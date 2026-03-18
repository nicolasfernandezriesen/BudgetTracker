using BudgetTracker.Models;
using BudgetTracker.Repositories.UserRepository;
using BudgetTracker.ViewModels.User;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.RegularExpressions;
using UserEntity = BudgetTracker.Models.User;

namespace BudgetTracker.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<UserEntity> _userManager;
        private readonly SignInManager<UserEntity> _signInManager;

        public UserService(
            IUserRepository userRepository,
            UserManager<UserEntity> userManager,
            SignInManager<UserEntity> signInManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<UserEntity?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<IEnumerable<UserEntity>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<UserEntity?> GetUserByNameAsync(string name)
        {
            return await _userRepository.GetUserByNameAsync(name);
        }

        public async Task UpdateUserAsync(EditViewModel editViewModel, int userId)
        {
            if (userId < 0)
            {
                throw new InvalidOperationException("User ID must be a non-negative integer.");
            }

            UserEntity user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            if (!string.IsNullOrEmpty(editViewModel.Password))
            {
                Regex passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*\d).{6,}$");
                var passwordCheck = await _userManager.CheckPasswordAsync(user, editViewModel.OldPassword);
                if (!passwordCheck)
                {
                    
                    throw new Exception("La contraseña actual es incorrecta.");
                }
                if (editViewModel.Password != editViewModel.ConfirmPassword)
                {
                    throw new Exception("Las contraseñas no coinciden.");
                }
                if (editViewModel.Password == editViewModel.OldPassword)
                {
                    throw new Exception("La nueva contraseña no puede ser igual a la contraseña actual.");
                }
                if (!passwordRegex.IsMatch(editViewModel.Password))
                {
                    throw new Exception("La contraseña no cumple con los requisitos de seguridad.");
                }
                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, editViewModel.OldPassword, editViewModel.Password);
                if (!passwordChangeResult.Succeeded)
                {
                    var errors = passwordChangeResult.Errors.Select(e => e.Description);
                    throw new Exception($"No se pudo cambiar la contraseña: {string.Join("\n\n ", errors)}");
                }
            }

            user.UserName = editViewModel.UserName;
            user.Email = editViewModel.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"No se pudo actualizar el usuario: {errors}");
            }
            else
            {
                await _signInManager.RefreshSignInAsync(user);
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordViewModel model)
        {
            string decodedEmail;
            try
            {
                var emailBytes = WebEncoders.Base64UrlDecode(model.Email);
                decodedEmail = Encoding.UTF8.GetString(emailBytes);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("El formato del email en la URL es inválido.");
            }

            var user = await _userManager.FindByEmailAsync(decodedEmail);
            if (user == null)
            {
                throw new InvalidOperationException("Hubo un problema, intentelo de nuevo.");
            }

            string decodedToken;
            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("El token de seguridad ha sido alterado o es inválido.");
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!resetPassResult.Succeeded)
            {
                var errors = string.Join(" ", resetPassResult.Errors.Select(e => e.Description));
                throw new Exception($"No se pudo restablecer la contraseña: {errors}");
            }

            await _signInManager.SignInAsync(user, isPersistent: true);
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
