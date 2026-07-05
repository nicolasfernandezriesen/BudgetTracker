using BudgetTracker.Models;
using BudgetTracker.ViewModels.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.ViewComponents
{
    public class ThemeViewComponent : ViewComponent
    {
        private readonly UserManager<User> _userManager;

        public ThemeViewComponent(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new ThemeViewModel
            {
                IsAuthenticated = User.Identity?.IsAuthenticated == true
            };

            if (model.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    model.IsTestRole = roles.Contains("Test");
                    model.IsDarkTheme = model.IsTestRole ? false : user.IsDarkTheme;
                }
            }

            return View(model);
        }
    }
}
