using BudgetTracker.Models;
using BudgetTracker.ViewModels.User;
using BudgetTracker.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, UserManager<User> userManager, ILogger<UserController> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: UserController
        public ActionResult Index(bool fromEmailConfirmation = false)
        {
            TempData["FromEmailConfirmation"] = fromEmailConfirmation;
            return View("Home");
        }

        // GET: UserController/GetUsers
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        private async Task<int> GetUserIDAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }

            return user.Id;
        }

        // GET: UserController/Edit
        public async Task<ActionResult> Edit()
        {
            _logger.LogInformation("Inicio de vista de edición de usuario. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            try
            {
                int userId = await GetUserIDAsync();
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("No se encontró usuario para edición. UserId: {UserId}. TraceId: {TraceId}", userId, HttpContext.TraceIdentifier);
                    return NotFound();
                }

                ViewBag.UserRole = await _userManager.GetRolesAsync(user);

                EditViewModel editViewModel = new EditViewModel
                {
                    Email = user.Email,
                    UserName = user.UserName
                };

                _logger.LogInformation("Vista de edición de usuario preparada. UserId: {UserId}. TraceId: {TraceId}", userId, HttpContext.TraceIdentifier);
                return View(editViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar vista de edición de usuario. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] EditViewModel editViewModel)
        {
            _logger.LogInformation("Inicio de actualización de usuario. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Actualización de usuario rechazada por modelo inválido. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = string.Join("\n\n ", errors) });
            }

            try
            {
                int userId = await GetUserIDAsync();
                await _userService.UpdateUserAsync(editViewModel, userId);

                _logger.LogInformation("Usuario actualizado correctamente. UserId: {UserId}. TraceId: {TraceId}", userId, HttpContext.TraceIdentifier);
                return Ok(new { Message = "Los cambios se guardaron correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Actualización de usuario falló por regla de negocio. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar usuario. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = ex.Message });
            }
        }

        //GET: UserController/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            _logger.LogInformation("Inicio de vista de reset de contraseña. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            if (token == null || email == null)
            {
                _logger.LogWarning("ResetPassword GET rechazado por token/email faltante. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = "Token o email no proporcionado." });
            }

            ResetPasswordViewModel model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            _logger.LogInformation("Vista de reset preparada correctamente. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return View(model);
        }

        //POST: UserController/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            _logger.LogInformation("Inicio de POST reset de contraseña. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ResetPassword POST rechazado por modelo inválido. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = string.Join("\n\n ", errors) });
            }
            try
            {
                await _userService.ResetPasswordAsync(model);
                _logger.LogInformation("Reset de contraseña completado exitosamente. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return Ok(new { Message = "Contraseña restablecida correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "ResetPassword POST falló por regla de negocio. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en ResetPassword POST. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = ex.Message });
            }
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
