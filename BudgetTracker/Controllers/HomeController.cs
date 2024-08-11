using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BudgetTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BudgettrackerdbContext context;

        public HomeController(ILogger<HomeController> logger, BudgettrackerdbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

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
                    return View();
                }

                context.Users.Add(newUser);
                context.SaveChanges();

                var claims = new List<Claim>
                {
                new Claim(ClaimTypes.Name, newUser.UserName),
                new Claim(ClaimTypes.Email, newUser.UserEmail)
                };

                // Crear una identidad de usuario
                var userIdentity = new ClaimsIdentity(claims, "userLoged");

                // Crear un principal de usuario
                var userPrincipal = new ClaimsPrincipal(userIdentity);

                // Agregar la cookie de validación a la respuesta
                await HttpContext.SignInAsync(userPrincipal);

                return RedirectToAction("Index", "User");
            }
            catch
            {
                return View();
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
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.UserEmail)
            };

            // Crear una identidad de usuario
            var userIdentity = new ClaimsIdentity(claims, "userLoged");

            // Crear un principal de usuario
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            // Agregar la cookie de validación a la respuesta
            await HttpContext.SignInAsync(userPrincipal);

            return RedirectToAction("Index", "User");
        }

        // GET: HomeController/Logout
        public async Task<IActionResult> Logout()
        {
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
