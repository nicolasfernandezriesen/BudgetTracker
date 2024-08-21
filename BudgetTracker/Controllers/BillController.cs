using BudgetTracker.Data;
using BudgetTracker.Models;
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

        private int GetUserID()
        {
            var encryptedUserId = Request.Cookies["UserId"];
            var stringId = _protector.Unprotect(encryptedUserId);
            return int.Parse(stringId);
        }

        // GET: BillController
        public ActionResult Index()
        {
            return View();
        }

        // GET: BillController/Details/5
        public ActionResult Details(int id)
        {
            return View();
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
        public ActionResult Create([Bind("BillsDate,BillsAmount,BillDesc,CategoryId")] Bill bill)
        {
            try
            {
                CheckIsValid(bill);

                int userId = GetUserID();

                bill.UserId = userId;

                context.Bills.Add(bill);
                context.SaveChanges();

                return Ok(new { message = "El nuevo gasto se ha guardado correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        private void CheckIsValid(Bill bill)
        {
            if (bill.BillsDate > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("La fecha no puede ser en el futuro.");
            }
            if (bill.BillsAmount <= 0)
            {
                throw new ArgumentException("El monto del gasto debe ser mayor a 0.");
            }
        }

        // GET: BillController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BillController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BillController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BillController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
