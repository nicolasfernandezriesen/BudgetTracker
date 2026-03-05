using BudgetTracker.Models;
using BudgetTracker.Services.User;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BudgetTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: HomeController/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: HomeController/Create
        public async Task<IActionResult> Create()
        {
            await _signInManager.SignOutAsync();
            return View();
        }

        // POST: HomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string email, string password)
        {
            try
            {
                if (!IsValidEmail(email))
                {
                    return BadRequest(new { Message = "The email is not valid." });
                }

                var newUser = new User
                {
                    UserName = username,
                    Email = email
                };

                var result = await _userManager.CreateAsync(newUser, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { Message = $"User registration failed: {errors}" });
                }

                await _userManager.AddToRoleAsync(newUser, "User");

                await _signInManager.SignInAsync(newUser, isPersistent: true);
                return Ok(new { Message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // GET: HomeController/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: HomeController/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest(new { Message = "Invalid mail." });
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent: true, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "Login successful." });
                }

                if (result.IsLockedOut)
                {
                    return BadRequest(new { Message = "Account is locked due to multiple failed login attempts." });
                }

                if (result.RequiresTwoFactor)
                {
                    return BadRequest(new { Message = "Two-factor authentication is required." });
                }

                return BadRequest(new { Message = "Invalid credentials." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // GET: HomeController/Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        // GET: HomeController/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
