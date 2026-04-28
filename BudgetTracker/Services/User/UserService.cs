using BudgetTracker.Repositories.UserRepository;
using BudgetTracker.ViewModels.User;
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
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            UserManager<UserEntity> userManager,
            SignInManager<UserEntity> signInManager,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
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
            _logger.LogInformation("Inicio de actualización de usuario. UserId: {UserId}", userId);

            if (userId < 0)
            {
                _logger.LogWarning("Actualización rechazada por UserId inválido. UserId: {UserId}", userId);
                throw new InvalidOperationException("User ID must be a non-negative integer.");
            }

            UserEntity user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Actualización fallida: usuario no encontrado. UserId: {UserId}", userId);
                throw new InvalidOperationException("Usuario no encontrado.");
            }

            if (!string.IsNullOrEmpty(editViewModel.Password))
            {
                Regex passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*\d).{6,}$");
                var passwordCheck = await _userManager.CheckPasswordAsync(user, editViewModel.OldPassword);
                if (!passwordCheck)
                {
                    _logger.LogWarning("Actualización fallida por contraseña actual inválida. UserId: {UserId}", userId);
                    throw new Exception("La contraseña actual es incorrecta.");
                }
                if (editViewModel.Password != editViewModel.ConfirmPassword)
                {
                    _logger.LogWarning("Actualización fallida por confirmación de contraseña inválida. UserId: {UserId}", userId);
                    throw new Exception("Las contraseñas no coinciden.");
                }
                if (editViewModel.Password == editViewModel.OldPassword)
                {
                    _logger.LogWarning("Actualización fallida por reutilización de contraseña. UserId: {UserId}", userId);
                    throw new Exception("La nueva contraseña no puede ser igual a la contraseña actual.");
                }
                if (!passwordRegex.IsMatch(editViewModel.Password))
                {
                    _logger.LogWarning("Actualización fallida por política de contraseña. UserId: {UserId}", userId);
                    throw new Exception("La contraseña no cumple con los requisitos de seguridad.");
                }
                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, editViewModel.OldPassword, editViewModel.Password);
                if (!passwordChangeResult.Succeeded)
                {
                    var errors = passwordChangeResult.Errors.Select(e => e.Description);
                    var errorMessage = string.Join("\n\n ", errors);
                    _logger.LogWarning("ChangePasswordAsync falló. UserId: {UserId}. Errores: {Errors}", userId, errorMessage);
                    throw new Exception($"No se pudo cambiar la contraseña: {errorMessage}");
                }
            }

            user.UserName = editViewModel.UserName;
            user.Email = editViewModel.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("UpdateAsync falló. UserId: {UserId}. Errores: {Errors}", userId, errors);
                throw new Exception($"No se pudo actualizar el usuario: {errors}");
            }
            else
            {
                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("Actualización de usuario completada correctamente. UserId: {UserId}", userId);
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordViewModel model)
        {
            _logger.LogInformation("Inicio de reset de contraseña desde servicio.");

            string decodedEmail;
            try
            {
                var emailBytes = WebEncoders.Base64UrlDecode(model.Email);
                decodedEmail = Encoding.UTF8.GetString(emailBytes);
            }
            catch (FormatException)
            {
                _logger.LogWarning("Reset fallido: email en formato inválido.");
                throw new InvalidOperationException("El formato del email en la URL es inválido.");
            }

            var user = await _userManager.FindByEmailAsync(decodedEmail);
            if (user == null)
            {
                _logger.LogWarning("Reset fallido: usuario no encontrado para email provisto.");
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
                _logger.LogWarning("Reset fallido: token con formato inválido. UserId: {UserId}", user.Id);
                throw new InvalidOperationException("El token de seguridad ha sido alterado o es inválido.");
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!resetPassResult.Succeeded)
            {
                var errors = string.Join(" ", resetPassResult.Errors.Select(e => e.Description));
                _logger.LogWarning("ResetPasswordAsync falló. UserId: {UserId}. Errores: {Errors}", user.Id, errors);
                throw new Exception($"No se pudo restablecer la contraseña: {errors}");
            }

            await _signInManager.SignInAsync(user, isPersistent: true);
            _logger.LogInformation("Reset de contraseña completado exitosamente. UserId: {UserId}", user.Id);
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
