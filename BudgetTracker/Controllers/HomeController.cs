using BudgetTracker.Models;
using BudgetTracker.Services.User;
using BudgetTracker.ViewModels;
using BudgetTracker.ViewModels.User;
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
        public async Task<IActionResult> Create([FromForm] CreateViewModel createUser)
        {
            if (!ModelState.IsValid) {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = string.Join("\n\n ", errors) });
            }

            try
            {
                var newUser = new User
                {
                    UserName = createUser.UserName,
                    Email = createUser.Email
                };

                var result = await _userManager.CreateAsync(newUser, createUser.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { Message = $"El registro del usuario falló: {errors}" });
                }

                await _userManager.AddToRoleAsync(newUser, "User");

                await _signInManager.SignInAsync(newUser, isPersistent: true);
                return Ok(new { Message = "Usuario registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
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
        public async Task<IActionResult> Login([FromForm] LoginViewModel loginUser)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = string.Join("\n\n ", errors) });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(loginUser.Email);
                if (user == null)
                {
                    return BadRequest(new { Message = "No se encontro el usuario." });
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, loginUser.Password, isPersistent: true, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Se ha iniciado sesión correctamente." });
                }

                if (result.IsLockedOut)
                {
                    return BadRequest(new { Message = "La cuenta está bloqueada debido a múltiples intentos fallidos de inicio de sesión." });
                }

                if (result.RequiresTwoFactor)
                {
                    return BadRequest(new { Message = "Se requiere autenticación de dos factores." });
                }

                return BadRequest(new { Message = "Credenciales no validas." });
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
