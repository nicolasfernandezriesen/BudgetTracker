using BudgetTracker.Models;
using BudgetTracker.Services.Income;
using BudgetTracker.ViewModels.Income;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class IncomeController : Controller
    {
        private readonly IIncomeService _incomeService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<IncomeController> _logger;

        public IncomeController(IIncomeService incomeService, UserManager<User> userManager, ILogger<IncomeController> logger)
        {
            _incomeService = incomeService;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task<int> GetUserIDAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return user.Id;
        }

        // GET: IncomeController
        public ActionResult Index()
        {
            return View("Home");
        }

        // GET: BillController/Details/"5/9/2024"
        public async Task<IActionResult> Details(string selectedDate)
        {
            try
            {
                var idUser = await GetUserIDAsync();
                DateOnly dateToSearch = DateOnly.Parse(selectedDate);

                var detailBillIncomeViewModel = await _incomeService.GetIncomeDetailsAsync(idUser, dateToSearch);
                return View("Details", detailBillIncomeViewModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: IncomeController/Create
        public async Task<IActionResult> Create(string selectedDate = null)
        {
            _logger.LogInformation("Inicio de vista de creación de ingreso. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            try
            {
                CreateViewModel viewModel = await _incomeService.GetCreateViewModelAsync();

                if (!string.IsNullOrEmpty(selectedDate))
                {
                    DateOnly date = DateOnly.Parse(selectedDate);
                    viewModel.IncomeDate = date;
                }

                _logger.LogInformation("Vista de creación de ingreso preparada. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar vista de creación de ingreso. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: IncomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateViewModel viewModel)
        {
            _logger.LogInformation("Inicio de creación de ingreso. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Creación de ingreso rechazada por validación. Errores: {Errors}. TraceId: {TraceId}", string.Join(", ", errors), HttpContext.TraceIdentifier);
                return BadRequest(new { Message = string.Join("\n\n ", errors) });
            }

            try
            {
                int userId = await GetUserIDAsync();
                await _incomeService.CreateIncomeAsync(userId, viewModel);
                _logger.LogInformation("Ingreso creado correctamente. UserId: {UserId}. TraceId: {TraceId}", userId, HttpContext.TraceIdentifier);
                return Ok(new { message = "The new income has been saved successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Creación de ingreso rechazada por validación. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear ingreso. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: IncomeController/Edit/?id=4&selectedDate=8%2F9%2F2024
        public async Task<IActionResult> Edit(int id, string selectedDate)
        {
            _logger.LogInformation("Inicio de vista de edición de ingreso. IncomeId: {IncomeId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);

            try
            {
                int userId = await GetUserIDAsync();
                DateOnly date = DateOnly.Parse(selectedDate);

                var viewModel = await _incomeService.GetEditViewModelAsync(userId, id);
                _logger.LogInformation("Vista de edición de ingreso preparada. IncomeId: {IncomeId}. UserId: {UserId}. TraceId: {TraceId}", id, userId, HttpContext.TraceIdentifier);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar edición de ingreso. IncomeId: {IncomeId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditViewModel viewModel)
        {
            _logger.LogInformation("Inicio de actualización de ingreso. IncomeId: {IncomeId}. TraceId: {TraceId}", viewModel.IncomeId, HttpContext.TraceIdentifier);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Actualización de ingreso rechazada por validación. Errores: {Errors}. TraceId: {TraceId}", string.Join(", ", errors), HttpContext.TraceIdentifier);
                return BadRequest(new { Message = string.Join("\n\n ", errors) });
            }

            try
            {
                int userId = await GetUserIDAsync();
                await _incomeService.UpdateIncomeAsync(userId, viewModel.IncomeId, viewModel);
                _logger.LogInformation("Ingreso actualizado correctamente. IncomeId: {IncomeId}. UserId: {UserId}. TraceId: {TraceId}", viewModel.IncomeId, userId, HttpContext.TraceIdentifier);
                return Ok(new { message = "The income has been successfully updated." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Actualización de ingreso falló por regla de negocio. IncomeId: {IncomeId}. TraceId: {TraceId}", viewModel.IncomeId, HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Actualización de ingreso rechazada por validación. IncomeId: {IncomeId}. TraceId: {TraceId}", viewModel.IncomeId, HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar ingreso. IncomeId: {IncomeId}. TraceId: {TraceId}", viewModel.IncomeId, HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: BillController/Delete/?id=4&selectedDate=8%2F9%2F2024
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string selectedDate)
        {
            try
            {
                int userId = await GetUserIDAsync();
                DateOnly date = DateOnly.Parse(selectedDate);

                await _incomeService.DeleteIncomeAsync(userId, id, date);
                return Ok(new { message = "The income has been successfully deleted." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
