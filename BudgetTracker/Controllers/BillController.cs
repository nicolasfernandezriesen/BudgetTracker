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

        public BillController(IBillService billService, UserManager<User> userManager)
        {
            _billService = billService;
            _userManager = userManager;
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
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = await _billService.GetCreateViewModelAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: BillController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int amount, int categoryId, string desc, DateOnly date)
        {
            try
            {
                int userId = await GetUserIDAsync();
                await _billService.CreateBillAsync(userId, amount, categoryId, desc, date);
                return Ok(new { message = "The bill was saved." });
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

        // GET: BillController/Edit/?id=4&selectedDate=8%2F9%2F2024
        public async Task<IActionResult> Edit(int id, string selectedDate)
        {
            try
            {
                int userId = await GetUserIDAsync();
                DateOnly date = DateOnly.Parse(selectedDate);

                var viewModel = await _billService.GetEditViewModelAsync(id, userId, date);
                return View(viewModel);
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

        // POST: BillController/Edit?selectedDate=8/9/2024
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int amount, int categoryId, string desc, DateOnly date, int id)
        {
            try
            {
                int userId = await GetUserIDAsync();
                await _billService.UpdateBillAsync(userId, id, amount, categoryId, desc, date);
                return Ok(new { message = "The expense has been updated successfully." });
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
