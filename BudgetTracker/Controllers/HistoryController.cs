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
            if (month == 0 || year == 0)
            {
                return BadRequest(new { Message = "Mes o año no válidos." });
            }

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

                foreach (var c in categories)
                {
                    var totalIncomeCat = history.Income
                        .Where(i => i.CategoryId == c.CategoryId)
                        .Sum(i => (decimal?)i.IncomeAmount) ?? 0;

                    var totalBillCat = history.Bill
                        .Where(b => b.CategoryId == c.CategoryId)
                        .Sum(b => (decimal?)b.BillsAmount) ?? 0;

                    if (totalIncomeCat != 0)
                    {
                        historyViewModel.CategoriesIncomes.Add(new CategoryIncomeDto
                        {
                            CategoryName = c.CategoryName,
                            TotalIncome = totalIncomeCat
                        });
                    }

                    if (totalBillCat != 0)
                    {
                        historyViewModel.CategoriesBills.Add(new CategoryBillDto
                        {
                            CategoryName = c.CategoryName,
                            TotalBill = totalBillCat
                        });
                    }
                }

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