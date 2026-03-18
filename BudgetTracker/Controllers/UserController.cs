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

        public UserController(IUserService userService, UserManager<User> userManager)
        {
            _userService = userService;
            _userManager = userManager;
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
            try
            {
                int userId = await GetUserIDAsync();
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                ViewBag.UserRole = await _userManager.GetRolesAsync(user);

                EditViewModel editViewModel = new EditViewModel
                {
                    Email = user.Email,
                    UserName = user.UserName
                };

                return View(editViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] EditViewModel editViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = string.Join("\n\n ", errors) });
            }

            try
            {
                int userId = await GetUserIDAsync();
                await _userService.UpdateUserAsync(editViewModel, userId);

                return Ok(new { Message = "Los cambios se guardaron correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        //GET: UserController/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return BadRequest(new { Message = "Token o email no proporcionado." });
            }

            ResetPasswordViewModel model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        //POST: UserController/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = string.Join("\n\n ", errors) });
            }
            try
            {
                await _userService.ResetPasswordAsync(model);
                return Ok(new { Message = "Contraseña restablecida correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
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
