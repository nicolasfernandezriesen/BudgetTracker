using BudgetTracker.Models;
using BudgetTracker.Services.History;
using BudgetTracker.Services.Category;
using BudgetTracker.ViewModels.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<User> _userManager;

        public HistoryController(IHistoryService historyService, ICategoryService categoryService, UserManager<User> userManager)
        {
            _historyService = historyService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        private async Task<int> GetUserIDAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Usuario no autenticado.");
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

        //GET: HistoryController/Details?month=5&year=2021
        public async Task<IActionResult> Details(int month, int year)
        {
            try
            {
                var idUser = await GetUserIDAsync();
                var history = await _historyService.GetBillIncomeAndMonthlyTotalAsync(idUser, month, year);
                var categories = await _categoryService.GetAllCategoriesAsync();

                DetailsViewModel historyViewModel = new DetailsViewModel();

                historyViewModel.Month = month;
                historyViewModel.Year = year;
                historyViewModel.TotalIncome = history.MonthlyTotal?.TotalIncome ?? 0;
                historyViewModel.TotalBill = history.MonthlyTotal?.TotalBill ?? 0;

                foreach (Income income in history.Income)
                {
                    historyViewModel.Transactions.Add(new Transaction
                    {
                        Date = income.IncomeDate,
                        Amount = income.IncomeAmount,
                        Description = income?.IncomeDesc  ?? "Sin descripción",
                        Category = categories.FirstOrDefault(c => c.CategoryId == income.CategoryId)?.CategoryName ?? "Sin categoría",
                        Type = TransactionType.Ingreso
                    });
                };

                foreach (Bill bill in history.Bill)
                {
                    historyViewModel.Transactions.Add(new Transaction
                    {
                        Date = bill.BillsDate,
                        Amount = bill.BillsAmount,
                        Description = bill?.BillsDesc  ?? "Sin descripción",
                        Category = categories?.FirstOrDefault(c => c.CategoryId == bill.CategoryId)?.CategoryName ?? "Sin categoría",
                        Type = TransactionType.Gasto
                    });
                }

                return View("Details", historyViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}