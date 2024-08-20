using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace BudgetTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BudgettrackerdbContext context;
        private readonly IDataProtector _protector;

        public HomeController(ILogger<HomeController> logger, BudgettrackerdbContext context, IDataProtectionProvider provider)
        {
            _logger = logger;
            this.context = context;
            _protector = provider.CreateProtector("UserIdProtector");
        }

        private ClaimsPrincipal CreateCookies (User user)
        {
            // Encrypt the user ID
            var encryptedUserId = _protector.Protect(user.UserId.ToString());

            // Create a cookie with the encrypted user ID
            Response.Cookies.Append("userId", encryptedUserId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.UserEmail)
            };

            // Create a user identity
            var userIdentity = new ClaimsIdentity(claims, "userLoged");

            // Create a user principal
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            return userPrincipal;
        }

        // GET: HomeController/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: UserController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User newUser)
        {
            ViewBag.ErrorCreateUser = 0;
            int userNameOrEmailExists = 0;

            try
            {
                var userExists = context.Users.Where(u => u.UserEmail == newUser.UserEmail).FirstOrDefault();
                if (userExists != null)
                {
                    ViewBag.ErrorMessage += "Este correo ya está registrado. ";
                    ViewBag.ErrorCreateUser = 1;
                    userNameOrEmailExists = 1;
                }

                userExists = context.Users.Where(u => u.UserName == newUser.UserName).FirstOrDefault();
                if (userExists != null)
                {
                    ViewBag.ErrorMessage += "Este nombre de usuario ya está en uso. ";
                    ViewBag.ErrorCreateUser = 1;
                    userNameOrEmailExists = 1;
                }

                if (userNameOrEmailExists > 0)
                {
                    return Conflict();
                };

                // Check if the email is valid
                if (!IsValidEmail(newUser.UserEmail))
                {
                    ViewBag.ErrorMessage += "El correo electrónico no es válido. ";
                    ViewBag.ErrorCreateUser = 1;
                    return BadRequest(new { Message = "El correo electrónico no es válido." });
                }

                context.Users.Add(newUser);
                context.SaveChanges();

                var userPrincipal = CreateCookies(newUser);

                // Add the validation cookie to the response
                await HttpContext.SignInAsync(userPrincipal);

                return Ok(new { Message = "Usuario registrado exitosamente." });
            }
            catch
            {
                return BadRequest(new { Message = "Ocurrió un error inesperado." });
            }
        }

        // Method to validate email
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

        // POST: UserController/Login
        [HttpPost]
        public async Task<IActionResult> Login(User userToLogin)
        {
            var user = context.Users.Where(u => u.UserEmail == userToLogin.UserEmail && u.UserPassword == userToLogin.UserPassword).FirstOrDefault();

            ViewBag.ErrorLogin = 0;

            if (user == null)
            {
                ViewBag.ErrorLogin = 1;
                ViewBag.ErrorMessage = "Usuario o contraseña incorrectos";
                return BadRequest(new { Message = "Credenciales invalidas." });
            };

            var userPrincipal = CreateCookies(user);

            // Add the validation cookie to the response
            await HttpContext.SignInAsync(userPrincipal);

            return Ok("Credenciales verificada");
        }

        // GET: HomeController/Logout
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("userId");
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
