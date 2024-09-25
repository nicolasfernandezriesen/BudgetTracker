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
    public class BillController : Controller
    {
        private readonly BudgettrackerdbContext context;
        private readonly IDataProtector _protector;

        public BillController(BudgettrackerdbContext context, IDataProtectionProvider provider)
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

        private void CheckIsValid(int amount, DateOnly date)
        {
            if (date > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("La fecha no puede ser en el futuro.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("El monto del gasto debe ser mayor a 0.");
            }
        }

        #endregion

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
                var idUser = GetUserID();

                DateOnly dateToSearch = DateOnly.Parse(selectedDate);

                DetailBillIncomeViewModel detailBillIncomeViewModel = new DetailBillIncomeViewModel { };

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
            }
            catch (Exception)
            {
                throw;
            }
        }

        // GET: BillController/Create
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

                var viewModel = new BillViewModel
                {
                    Bill = new Bill
                    {
                        BillsDate = DateOnly.FromDateTime(DateTime.Now)
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

        // POST: BillController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int amount, int categoryId, string desc, DateOnly date)
        {
            try
            {
                CheckIsValid(amount, date);

                int userId = GetUserID();

                var bill = new Bill
                {
                    UserId = userId,
                    BillsAmount = amount,
                    BillsDesc = desc,
                    BillsDate = date,
                    CategoryId = categoryId
                };

                bill.UserId = userId;

                var monthlyTotal = GetOrCreateMonthlyTotal(bill.BillsDate.Month, userId);

                monthlyTotal.TotalBill += bill.BillsAmount;

                context.MonthlyTotals.Update(monthlyTotal);
                context.Bills.Add(bill);
                context.SaveChanges();

                return Ok(new { message = "El nuevo gasto se ha guardado correctamente." });
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

        // GET: BillController/Edit/?id=4&selectedDate=8%2F9%2F2024
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

                var bill = context.Bills
                    .FirstOrDefault(b => b.BillsId == id && b.UserId == userId && b.BillsDate == date);

                if (bill == null) {
                    return BadRequest(new { message = "The bill does not exist." });
                }

                var viewModel = new BillViewModel
                {
                    Bill = bill,
                    Categories = categories
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: BillController/Edit?selectedDate=8/9/2024
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int amount, int categoryId, string desc, DateOnly date, int id)
        {
            try
            {
                CheckIsValid(amount, date);

                int userId = GetUserID();

                var bill = await context.Bills
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.BillsId == id);

                if (bill == null)
                {
                    return BadRequest(new { message = "No se encontro el gasto." });
                }

                bill.BillsAmount = amount;
                bill.BillsDesc = desc;
                bill.BillsDate = date;
                bill.CategoryId = categoryId;

                var monthlyTotal = GetOrCreateMonthlyTotal(bill.BillsDate.Month, userId);

                monthlyTotal.TotalBill -= bill.BillsAmount;

                context.MonthlyTotals.Update(monthlyTotal);
                context.Bills.Update(bill);
                await context.SaveChangesAsync();

                return Ok(new { message = "El gasto se ha actualizado correctamente." });

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

            var bill = await context.Bills
                .FirstOrDefaultAsync(b => b.BillsId == id && b.UserId == userId && b.BillsDate == date);

            if (bill == null)
            {
                return BadRequest(new { message = "No se encontró el gasto." });
            }

            var monthlyTotal = await context.MonthlyTotals
                .FirstOrDefaultAsync(mt => mt.MonthlyTotalsMonth == bill.BillsDate.Month && mt.UserId == userId);

            if (monthlyTotal != null)
            {
                monthlyTotal.TotalBill -= bill.BillsAmount;
                context.MonthlyTotals.Update(monthlyTotal);
            }

            context.Bills.Remove(bill);
            await context.SaveChangesAsync();

            return Ok(new { message = "El gasto se ha eliminado correctamente." });
            }
            catch (Exception ex)
            {
            return BadRequest(ex.Message);
            }
        }
    }
}
