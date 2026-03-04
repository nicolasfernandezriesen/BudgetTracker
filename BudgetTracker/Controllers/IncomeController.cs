using BudgetTracker.Services.Income;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class IncomeController : Controller
    {
        private readonly IIncomeService _incomeService;

        public IncomeController(IIncomeService incomeService)
        {
            _incomeService = incomeService;
        }

        private int GetUserID()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return userId;
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
                var idUser = GetUserID();
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
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = await _incomeService.GetCreateViewModelAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: IncomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int amount, int categoryId, string desc, DateOnly date)
        {
            try
            {
                int userId = GetUserID();
                await _incomeService.CreateIncomeAsync(userId, amount, categoryId, desc, date);
                return Ok(new { message = "The new income has been saved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: IncomeController/Edit/?id=4&selectedDate=8%2F9%2F2024
        public async Task<IActionResult> Edit(int id, string selectedDate)
        {
            try
            {
                int userId = GetUserID();
                DateOnly date = DateOnly.Parse(selectedDate);

                var income = await _incomeService.GetIncomeByUserAsync(userId);
                var incomeItem = income.FirstOrDefault(i => i.IncomeId == id);

                if (incomeItem == null)
                {
                    return BadRequest(new { message = "The income does not exist." });
                }

                var categories = await _incomeService.GetCreateViewModelAsync();
                
                var viewModel = new IncomeCreateViewModel
                {
                    Income = incomeItem,
                    Categories = categories.Categories
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int amount, int categoryId, string desc, DateOnly date, int id)
        {
            try
            {
                int userId = GetUserID();
                await _incomeService.UpdateIncomeAsync(userId, id, amount, categoryId, desc, date);
                return Ok(new { message = "The income has been successfully updated." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
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
                int userId = GetUserID();
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
