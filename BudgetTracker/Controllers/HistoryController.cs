using BudgetTracker.Data;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly BudgettrackerdbContext context;
        private readonly IDataProtector _protector;

        public HistoryController(BudgettrackerdbContext context, IDataProtectionProvider provider)
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

        #endregion

        // GET: HistoryController/GetBillIncomeAndMonthlyTotal?month=5&year=2021
        public async Task<IActionResult> GetBillIncomeAndMonthlyTotal(int month, int year)
        {
            try
            {
                var idUser = GetUserID();

                var income = await context.Incomes
                    .Where(i => i.IncomeDate.Month == month && i.IncomeDate.Year == year && i.UserId == idUser)
                    .ToListAsync();

                var bill = await context.Bills
                    .Where(b => b.BillsDate.Month == month && b.BillsDate.Year == year && b.UserId == idUser)
                    .ToListAsync();

                var monthlyTotal = await context.MonthlyTotals
                    .FirstOrDefaultAsync(mt => mt.MonthlyTotalsMonth == month && mt.MonthlyTotalsYear == year && mt.UserId == idUser);

                var historyViewModel = new HistoryViewModel
                {
                    Income = income,
                    Bill = bill,
                    MonthlyTotal = monthlyTotal
                };

                return View("History", historyViewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
