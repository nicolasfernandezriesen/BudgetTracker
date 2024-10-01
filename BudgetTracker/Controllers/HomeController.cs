using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
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
            Response.Cookies.Delete("userId");
            HttpContext.SignOutAsync();
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string email, string password)
        {
            try
            {
                // Check if the email is valid
                if (!IsValidEmail(email))
                {
                    return BadRequest(new { Message = "The email is not valid." });
                }

                User newUser = new User { UserName = username, UserEmail = email, UserPassword = password };

                context.Users.Add(newUser);
                context.SaveChanges();

                var userPrincipal = CreateCookies(newUser);

                // Add the validation cookie to the response
                await HttpContext.SignInAsync(userPrincipal);

                return Ok(new { Message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message.Contains("user_email"))
                {
                    return BadRequest(new { Message = "The email is already registered." });
                }
                else if (ex.InnerException.Message.Contains("user_name"))
                {
                    return BadRequest(new { Message = "The username is already registered." });
                }
                else
                {
                    return BadRequest(ex.InnerException.Message);
                }
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

            if (user == null)
            {
                return BadRequest(new { Message = "Invalid credentials." });
            };

            var userPrincipal = CreateCookies(user);

            // Add the validation cookie to the response
            await HttpContext.SignInAsync(userPrincipal);

            return Ok("Credentials verified");
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
