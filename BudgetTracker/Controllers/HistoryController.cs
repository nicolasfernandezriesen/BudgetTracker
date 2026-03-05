using BudgetTracker.Models;
using BudgetTracker.Services.History;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;
        private readonly UserManager<User> _userManager;

        public HistoryController(IHistoryService historyService, UserManager<User> userManager)
        {
            _historyService = historyService;
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

        // GET: HistoryController/GetBillIncomeAndMonthlyTotal?month=5&year=2021
        public async Task<IActionResult> GetBillIncomeAndMonthlyTotal(int month, int year)
        {
            try
            {
                var idUser = await GetUserIDAsync();
                var historyViewModel = await _historyService.GetBillIncomeAndMonthlyTotalAsync(idUser, month, year);
                return View("History", historyViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
