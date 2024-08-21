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
    public class IncomeController : Controller
    {
        private readonly BudgettrackerdbContext context;
        private readonly IDataProtector _protector;

        public IncomeController(BudgettrackerdbContext context, IDataProtectionProvider provider)
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

        // GET: IncomeController
        public ActionResult Index()
        {
            return View("Home");
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
        public ActionResult Create([Bind("IncomeDate,IncomeAmount,IncomeDesc,CategoryId")] Income income)
        {
            try
            {
                CheckIsValid(income);

                int userId = GetUserID();

                income.UserId = userId;

                context.Incomes.Add(income);
                context.SaveChanges();

                return Ok(new { message = "El nuevo ingreso se ha guardado correctamente." });
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

        private void CheckIsValid(Income income)
        {
            if (income.IncomeDate > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ArgumentException("La fecha no puede ser en el futuro.");
            }
            if (income.IncomeAmount <= 0)
            {
                throw new ArgumentException("El monto del ingreso debe ser mayor a 0.");
            }
        }
    }
}
