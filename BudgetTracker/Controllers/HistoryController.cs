using BudgetTracker.Services.History;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
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

        // GET: HistoryController/GetBillIncomeAndMonthlyTotal?month=5&year=2021
        public async Task<IActionResult> GetBillIncomeAndMonthlyTotal(int month, int year)
        {
            try
            {
                var idUser = GetUserID();
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
