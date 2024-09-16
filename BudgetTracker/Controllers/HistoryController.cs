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

        // GET: HistoryController/GetBillIncomeAndMonthlyTotal
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

        //GET: HistoryController/GetDetails/5

        public async Task<IActionResult> GetDetails(string selectedDate, string typeDetail)
        {
            try
            {
                var idUser = GetUserID();

                DateOnly dateToSearch = DateOnly.Parse(selectedDate);

                DetailBillIncomeViewModel detailBillIncomeViewModel = new DetailBillIncomeViewModel { };

                switch (typeDetail)
                {
                    case "income":
                        var income = await context.Incomes
                            .Include(i => i.Category)
                            .Where(i => i.IncomeDate == dateToSearch && i.UserId == idUser)
                            .ToListAsync();

                        if(income == null)
                        {
                            return BadRequest(new { message = "The income/s does not exist." });
                        }

                        detailBillIncomeViewModel.Income = income;
                        detailBillIncomeViewModel.Date = dateToSearch;

                        return View("Details", detailBillIncomeViewModel);

                    case "bill":
                        var bill = await context.Bills
                            .Include(b => b.Category)
                            .Where(b => b.BillsDate == dateToSearch && b.UserId == idUser)
                            .ToListAsync();

                        if (bill == null)
                        {
                            return BadRequest(new { message = "The bill/s does not exist." });
                        }

                        detailBillIncomeViewModel.Bill = bill;
                        detailBillIncomeViewModel.Date = dateToSearch;

                        return View("Details", detailBillIncomeViewModel);

                    default:
                        return BadRequest(new { message = "The type of the details must be bill or income." });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
