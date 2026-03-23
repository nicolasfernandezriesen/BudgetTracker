using BudgetTracker.Models;
using BudgetTracker.ViewModels;
using BudgetTracker.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Text;

namespace BudgetTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // GET: HomeController/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: HomeController/Create
        [HttpGet]
        [AllowAnonymous]
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
            _logger.LogInformation("Inicio de registro de usuario. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            if (!ModelState.IsValid) {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Registro rechazado por modelo inválido. TraceId: {TraceId}", HttpContext.TraceIdentifier);
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
                    _logger.LogWarning("Registro fallido en Identity. TraceId: {TraceId}. Errores: {Errors}", HttpContext.TraceIdentifier, errors);
                    return BadRequest(new { Message = $"El registro del usuario falló: {errors}" });
                }

                await _userManager.AddToRoleAsync(newUser, "User");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

                var callbackUrl = Url.Action("ConfirmEmail", "Home", new { userId = newUser.Id, token }, protocol: HttpContext.Request.Scheme);
                string emailContent = $@"
                                    <h1>Bienvenido a Budget Tracker</h1>
                                    <p>Para activar tu cuenta, haz clic en el siguiente enlace:
                                        <a href='{callbackUrl}' style='padding:1px; color:black;'>Confirmar Cuenta</a>
                                    </p>";

                await _emailSender.SendEmailAsync(newUser.Email, "Confirmar cuenta", emailContent);
                _logger.LogInformation("Registro exitoso. UserId: {UserId}. TraceId: {TraceId}", newUser.Id, HttpContext.TraceIdentifier);
                return Ok(new { Message = "Usuario registrado exitosamente. Se ha enviado un mail para confirmar su cuenta." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante el registro. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = ex.Message });
            }
        }

        //GET : HomeController/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            _logger.LogInformation("Inicio de confirmación de email. UserId: {UserId}. TraceId: {TraceId}", userId, HttpContext.TraceIdentifier);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || token == null)
            {
                _logger.LogWarning("Confirmación de email inválida por usuario/token faltante. UserId: {UserId}. TraceId: {TraceId}", userId, HttpContext.TraceIdentifier);
                return RedirectToAction("Index", "Home");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
                _logger.LogInformation("Email confirmado correctamente. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                return RedirectToAction("Index", "User", new { fromEmailConfirmation = true });
            }
            else
            {
                _logger.LogWarning("Falló la confirmación de email. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                return BadRequest(new { Message = "Error al confirmar el email." });
            }
        }

        //GET: HomeControlller/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //POST: HomeController/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromForm] string email)
        {
            _logger.LogInformation("Inicio de solicitud de recuperación de contraseña. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            if (email == null)
            {
                _logger.LogWarning("Solicitud de recuperación rechazada por email nulo. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = "El email no puede ser nulo." });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    _logger.LogInformation("Solicitud de recuperación procesada sin revelar existencia de cuenta. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                    return Ok(new { Message = "Se ha enviado un correo de recuperación al email imgresado." });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var encodedEmail = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(email));
                var callbackUrl = Url.Action("ResetPassword", "User",
                    new { token = encodedToken, email = encodedEmail }, protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(email, "Restablecer contraseña",
                    $"Para restablecer tu contraseña, haz clic en el siguiente enlace: <a href='{callbackUrl}'>Restablecer contraseña</a>");

                _logger.LogInformation("Correo de recuperación enviado. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                return Ok(new { Message = "Se ha enviado un correo de recuperación al email ingresado." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar recuperación de contraseña. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = "No se pudo procesar la recuperación de contraseña." });
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
            _logger.LogInformation("Inicio de login. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Login rechazado por modelo inválido. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Error = "InvalidModel", Message = string.Join("\n\n ", errors) });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(loginUser.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login fallido por usuario inexistente. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                    return Unauthorized(new { Error = "InvalidCredentials", Message = "Credenciales no validas." });
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, loginUser.Password, isPersistent: true, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Login exitoso. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                    return Ok(new { Message = "Se ha iniciado sesión correctamente." });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Login bloqueado por lockout. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                    return StatusCode(423, new { Error = "AccountLocked", Message = "La cuenta está bloqueada debido a múltiples intentos fallidos de inicio de sesión." });
                }

                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("Mail no confirmado. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                    return StatusCode(403, new { Error = "EmailNotConfirmed", Message = "El mail no ha sido confirmado." });
                }
                
                if (result.RequiresTwoFactor) 
                {
                    _logger.LogWarning("Login requiere 2FA. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                    return Unauthorized(new { Error = "TwoFactorRequired", Message = "Se requiere autenticación de dos factores." });
                }

                _logger.LogWarning("Login fallido por credenciales inválidas. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                return Unauthorized(new { Error = "InvalidCredentials", Message = "Credenciales no validas." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante login. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: HomeController/ReSendConfirmationEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReSendConfirmationEmail([FromForm] string email)
        {
            _logger.LogInformation("Inicio de reenvío de email de confirmación. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            if (email == null)
            {
                _logger.LogWarning("Reenvío de confirmación rechazado por email nulo. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Error = "EmailNull", Message = "El email no puede ser nulo." });
            }
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogInformation("Reenvío de confirmación procesado sin revelar existencia de cuenta. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                    return Ok(new { Message = "Se ha enviado el mail de confirmacion a su cuenta email." });
                }
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Home", new { userId = user.Id, token }, protocol: HttpContext.Request.Scheme);
                string emailContent = $@"
                                    <h1>Bienvenido a Budget Tracker</h1>
                                    <p>Para activar tu cuenta, haz clic en el siguiente enlace:
                                        <a href='{callbackUrl}' style='padding:1px; color:black;'>Confirmar Cuenta</a>
                                    </p>";
                await _emailSender.SendEmailAsync(user.Email, "Confirmar cuenta", emailContent);
                _logger.LogInformation("Correo de confirmación reenviado. UserId: {UserId}. TraceId: {TraceId}", user.Id, HttpContext.TraceIdentifier);
                return Ok(new { Message = "Se ha enviado el mail de confirmacion a su cuenta email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar email de confirmación. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { Message = "No se pudo re-enviar el correo de confirmación." });
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
