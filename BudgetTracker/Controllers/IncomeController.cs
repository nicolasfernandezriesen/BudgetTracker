using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class IncomeController : Controller
    {
        private readonly BudgettrackerdbContext context;
        private readonly IDataProtector _protector;

        public IncomeController(BudgettrackerdbContext context, IDataProtectionProvider provider)
        {
            this.context = context;
            _protector = provider.CreateProtector("UserIdProtector");
        }

        #region Functions

        private int GetUserID()
        {
            var encryptedUserId = Request.Cookies["UserId"];
            var stringId = _protector.Unprotect(encryptedUserId);
            return int.Parse(stringId);
        }

        private void CheckIsValid(int amount, int categoryId, DateOnly date)
        {
            if (date > DateOnly.FromDateTime(DateTime.Now.AddMonths(2)))
            {
                throw new ArgumentException("The date cannot be greater than 2 months.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("The amount of income must be greater than 0.");
            }
            if (categoryId == 0)
            {
                throw new ArgumentException("You must choose a category.");
            }
        }

        private MonthlyTotal GetOrCreateMonthlyTotal(int month, int idUser)
        {
            var monthlyTotal = context.MonthlyTotals.FirstOrDefault(mt => mt.MonthlyTotalsMonth == month);

            if (monthlyTotal == null)
            {
                monthlyTotal = new MonthlyTotal
                {
                    MonthlyTotalsYear = DateTime.Now.Year,
                    MonthlyTotalsMonth = month,
                    TotalIncome = 0,
                    TotalBill = 0,
                    UserId = idUser
                };

                context.MonthlyTotals.Add(monthlyTotal);
                context.SaveChanges();
            }

            return monthlyTotal;
        }

        #endregion

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

                DetailBillIncomeViewModel detailBillIncomeViewModel = new DetailBillIncomeViewModel { };

                var income = await context.Incomes
                            .Include(i => i.Category)
                            .Where(i => i.IncomeDate == dateToSearch && i.UserId == idUser)
                            .ToListAsync();

                if (income == null)
                {
                    return BadRequest(new { message = "The income/s does not exist." });
                }

                detailBillIncomeViewModel.Income = income;
                detailBillIncomeViewModel.Date = dateToSearch;

                return View("Details", detailBillIncomeViewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // GET: IncomeController/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await context.Categorys
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToListAsync();

                var viewModel = new IncomeCreateViewModel
                {
                    Income = new Income
                    {
                        IncomeDate = DateOnly.FromDateTime(DateTime.Now)
                    },
                    Categories = categories
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // POST: IncomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int amount, int categoryId, string desc, DateOnly date)
        {
            try
            {
                CheckIsValid(amount, categoryId, date);

                int userId = GetUserID();

                var income = new Income
                {
                    IncomeAmount = amount,
                    CategoryId = categoryId,
                    IncomeDesc = desc,
                    IncomeDate = date,
                    UserId = userId
                };

                var monthlyTotal = GetOrCreateMonthlyTotal(income.IncomeDate.Month, userId);

                monthlyTotal.TotalIncome += income.IncomeAmount;

                context.MonthlyTotals.Update(monthlyTotal);
                context.Incomes.Add(income);
                context.SaveChanges();

                return Ok(new { message = "The new income has been saved successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: IncomeController/Edit/?id=4&selectedDate=8%2F9%2F2024
        public async Task<IActionResult> Edit(int id, string selectedDate)
        {
            try
            {
                int userId = GetUserID();
                DateOnly date = DateOnly.Parse(selectedDate);

                var categories = await context.Categorys
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToListAsync();

                var income = context.Incomes
                    .FirstOrDefault(i => i.IncomeId == id && i.UserId == userId && i.IncomeDate == date);

                if (income == null)
                {
                    return BadRequest(new { message = "The income does not exist." });
                }

                var viewModel = new IncomeCreateViewModel
                {
                    Income = income,
                    Categories = categories
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int amount, int categoryId, string desc, DateOnly date, int id)
        {
            try
            {
                CheckIsValid(amount, categoryId, date);

                int userId = GetUserID();

                var income = await context.Incomes
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.IncomeId == id);

                if (income == null)
                {
                    return BadRequest(new { message = "The income was not found." });
                }

                income.IncomeAmount = amount;
                income.IncomeDesc = desc;
                income.IncomeDate = date;
                income.CategoryId = categoryId;

                var monthlyTotal = GetOrCreateMonthlyTotal(income.IncomeDate.Month, userId);

                monthlyTotal.TotalIncome -= income.IncomeAmount;

                context.MonthlyTotals.Update(monthlyTotal);
                context.Incomes.Update(income);
                await context.SaveChangesAsync();

                return Ok(new { message = "The income has been successfully updated." });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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

                var income = await context.Incomes
                    .FirstOrDefaultAsync(b => b.IncomeId == id && b.UserId == userId && b.IncomeDate == date);

                if (income == null)
                {
                    return BadRequest(new { message = "The income was not found." });
                }

                var monthlyTotal = await context.MonthlyTotals
                    .FirstOrDefaultAsync(mt => mt.MonthlyTotalsMonth == income.IncomeDate.Month && mt.UserId == userId);

                if (monthlyTotal != null)
                {
                    monthlyTotal.TotalIncome -= income.IncomeAmount;
                    context.MonthlyTotals.Update(monthlyTotal);
                }

                context.Incomes.Remove(income);
                await context.SaveChangesAsync();

                return Ok(new { message = "The income has been successfully deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
