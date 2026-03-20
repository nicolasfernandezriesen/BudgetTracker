using BudgetTracker.Models;
using BudgetTracker.Services.Bill;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class BillController : Controller
    {
        private readonly IBillService _billService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<BillController> _logger;

        public BillController(IBillService billService, UserManager<User> userManager, ILogger<BillController> logger)
        {
            _billService = billService;
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

        // GET: BillController
        public ActionResult Index()
        {
            return View();
        }

        // GET: BillController/Details/"5/9/2024"
        public async Task<IActionResult> Details(string selectedDate)
        {
            try
            {
                var idUser = await GetUserIDAsync();
                DateOnly dateToSearch = DateOnly.Parse(selectedDate);

                var detailBillIncomeViewModel = await _billService.GetBillDetailsAsync(idUser, dateToSearch);
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

        // GET: BillController/Create
        public async Task<IActionResult> Create(string selectedDate = null)
        {
            _logger.LogInformation("Inicio de vista de creación de gasto. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            try
            {
                var viewModel = await _billService.GetCreateViewModelAsync();

                if (!string.IsNullOrEmpty(selectedDate))
                {
                    DateOnly date = DateOnly.Parse(selectedDate);
                    viewModel.Bill.BillsDate = date;
                }

                _logger.LogInformation("Vista de creación de gasto preparada. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar vista de creación de gasto. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: BillController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int amount, int categoryId, string desc, DateOnly date)
        {
            _logger.LogInformation("Inicio de creación de gasto. TraceId: {TraceId}", HttpContext.TraceIdentifier);

            try
            {
                int userId = await GetUserIDAsync();
                await _billService.CreateBillAsync(userId, amount, categoryId, desc, date);
                _logger.LogInformation("Gasto creado correctamente. UserId: {UserId}. TraceId: {TraceId}", userId, HttpContext.TraceIdentifier);
                return Ok(new { message = "The bill was saved." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Creación de gasto rechazada por validación. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear gasto. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: BillController/Edit/?id=4&selectedDate=8%2F9%2F2024
        public async Task<IActionResult> Edit(int id, string selectedDate)
        {
            _logger.LogInformation("Inicio de vista de edición de gasto. BillId: {BillId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);

            try
            {
                int userId = await GetUserIDAsync();
                DateOnly date = DateOnly.Parse(selectedDate);

                var viewModel = await _billService.GetEditViewModelAsync(id, userId, date);
                _logger.LogInformation("Vista de edición de gasto preparada. BillId: {BillId}. UserId: {UserId}. TraceId: {TraceId}", id, userId, HttpContext.TraceIdentifier);
                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "No se pudo preparar edición de gasto por regla de negocio. BillId: {BillId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar edición de gasto. BillId: {BillId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: BillController/Edit?selectedDate=8/9/2024
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int amount, int categoryId, string desc, DateOnly date, int id)
        {
            _logger.LogInformation("Inicio de actualización de gasto. BillId: {BillId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);

            try
            {
                int userId = await GetUserIDAsync();
                await _billService.UpdateBillAsync(userId, id, amount, categoryId, desc, date);
                _logger.LogInformation("Gasto actualizado correctamente. BillId: {BillId}. UserId: {UserId}. TraceId: {TraceId}", id, userId, HttpContext.TraceIdentifier);
                return Ok(new { message = "The expense has been updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Actualización de gasto falló por regla de negocio. BillId: {BillId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Actualización de gasto rechazada por validación. BillId: {BillId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar gasto. BillId: {BillId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
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

                await _billService.DeleteBillAsync(userId, id, date);
                return Ok(new { message = "The expense has been successfully deleted." });
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
