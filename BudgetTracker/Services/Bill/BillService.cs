using BudgetTracker.Repositories.BillRepository;
using BudgetTracker.Repositories.CategoryRepository;
using BudgetTracker.Repositories.MonthlyTotalRepository;
using BillModel = BudgetTracker.Models.Bill;
using MonthlyTotalModel = BudgetTracker.Models.MonthlyTotal;
using BudgetTracker.ViewModels;
using BudgetTracker.ViewModels.Bill;
using BudgetTracker.ViewModels.Category;

namespace BudgetTracker.Services.Bill
{
    public class BillService : IBillService
    {
        private readonly IBillRepository _billRepository;
        private readonly IMonthlyTotalRepository _monthlyTotalRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<BillService> _logger;

        public BillService(IBillRepository billRepository, IMonthlyTotalRepository monthlyTotalRepository, ICategoryRepository categoryRepository, ILogger<BillService> logger)
        {
            _billRepository = billRepository;
            _monthlyTotalRepository = monthlyTotalRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<BillModel>> GetBillsByUserAsync(int userId)
        {
            return await _billRepository.GetBillsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<BillModel>> GetBillsByDateAsync(int userId, DateOnly date)
        {
            return await _billRepository.GetBillsByUserAndDateAsync(userId, date);
        }

        public async Task<DetailBillIncomeViewModel> GetBillDetailsAsync(int userId, DateOnly date)
        {
            var bills = await _billRepository.GetBillsByUserAndDateAsync(userId, date);
            
            if (!bills.Any())
            {
                throw new InvalidOperationException("The bill/s does not exist.");
            }

            return new DetailBillIncomeViewModel
            {
                Bill = bills.ToList(),
                Date = date
            };
        }

        public async Task<CreateViewModel> GetCreateViewModelAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var groupedCategories = categories
                .Where(c => c.parentcategoryid != null)
                .GroupBy(c => categories.First(p => p.CategoryId == c.parentcategoryid).CategoryName)
                .Select(g => new CategoryGroupViewModel
                {
                    GroupName = g.Key,
                    SubCategories = g.ToList()
                }).ToList();

            return new CreateViewModel 
            {
                AvailableCategories = groupedCategories
            };
        }

        public async Task<EditViewModel> GetEditViewModelAsync(int billId, int userId, DateOnly date)
        {
            var bill = await _billRepository.GetBillByIdAndUserAsync(billId, userId);
            if (bill == null)
            {
                throw new InvalidOperationException("The bill does not exist.");
            }

            var categories = await _categoryRepository.GetAllAsync();
            var groupedCategories = categories
                .Where(c => c.parentcategoryid != null)
                .GroupBy(c => categories.First(p => p.CategoryId == c.parentcategoryid).CategoryName)
                .Select(g => new CategoryGroupViewModel
                {
                    GroupName = g.Key,
                    SubCategories = g.ToList()
                }).ToList();

            return new EditViewModel
            {
                BillId = bill.BillsId,
                BillsDate = bill.BillsDate,
                BillsAmount = bill.BillsAmount,
                BillsDesc = bill.BillsDesc,
                CategoryId = bill.CategoryId,
                AvailableCategories = groupedCategories
            };
        }

        public async Task CreateBillAsync(int userId, CreateViewModel model)
        {
            _logger.LogInformation("Inicio de CreateBillAsync. UserId: {UserId}", userId);

            try
            {
                ValidateBillInput(model.BillsAmount, model.CategoryId, model.BillsDate);

                var bill = new BillModel
                 {
                    UserId = userId,
                    BillsAmount = model.BillsAmount,
                    BillsDesc = model.BillsDesc,
                    BillsDate = model.BillsDate,
                    CategoryId = model.CategoryId
                };

                var monthlyTotal = await GetOrCreateMonthlyTotalAsync(model.BillsDate.Month, model.BillsDate.Year, userId);
                monthlyTotal.TotalBill += bill.BillsAmount;

                await _billRepository.AddAsync(bill);
                _monthlyTotalRepository.Update(monthlyTotal);
                await _billRepository.SaveChangesAsync();

                _logger.LogInformation("CreateBillAsync completado. UserId: {UserId}. Amount: {Amount}. CategoryId: {CategoryId}", userId, bill.BillsAmount, bill.CategoryId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "CreateBillAsync rechazado por validación. UserId: {UserId}", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateBillAsync. UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateBillAsync(int userId, EditViewModel model)
        {
            _logger.LogInformation("Inicio de UpdateBillAsync. UserId: {UserId}. BillId: {BillId}", userId, model.BillId);

            try
            {
                ValidateBillInput(model.BillsAmount, model.CategoryId, model.BillsDate);
                var bill = await _billRepository.GetBillByIdAndUserAsync(model.BillId, userId);
                if (bill == null)
                {
                    _logger.LogWarning("UpdateBillAsync falló: gasto no encontrado. UserId: {UserId}. BillId: {BillId}", userId, model.BillId);
                    throw new InvalidOperationException("Ocurrio un error con el gasto. Intentelo de nuevo.");
                }

                var monthlyTotal = await GetOrCreateMonthlyTotalAsync(bill.BillsDate.Month, bill.BillsDate.Year, userId);
                monthlyTotal.TotalBill -= bill.BillsAmount;
                monthlyTotal.TotalBill += model.BillsAmount;

                bill.BillsAmount = model.BillsAmount;
                bill.BillsDesc = model.BillsDesc;
                bill.BillsDate = model.BillsDate;
                bill.CategoryId = model.CategoryId;

                _monthlyTotalRepository.Update(monthlyTotal);
                _billRepository.Update(bill);
                await _billRepository.SaveChangesAsync();

                _logger.LogInformation("UpdateBillAsync completado. UserId: {UserId}. BillId: {BillId}. Amount: {Amount}. CategoryId: {CategoryId}", userId, model.BillId, model.BillsAmount, model.CategoryId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "UpdateBillAsync rechazado por validación. UserId: {UserId}. BillId: {BillId}", userId, model.BillId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UpdateBillAsync. UserId: {UserId}. BillId: {BillId}", userId, model.BillId);
                throw;
            }
        }

        public async Task DeleteBillAsync(int userId, int billId, DateOnly date)
        {
            var bill = await _billRepository.GetBillByIdAndUserAsync(billId, userId);
            if (bill == null)
            {
                throw new InvalidOperationException("Ocurrio un error con el gasto. Intentelo de nuevo.");
            }

            var monthlyTotal = await _monthlyTotalRepository.GetMonthlyTotalByMonthAndUserAsync(
                bill.BillsDate.Month, bill.BillsDate.Year, userId);

            if (monthlyTotal != null)
            {
                monthlyTotal.TotalBill -= bill.BillsAmount;
                _monthlyTotalRepository.Update(monthlyTotal);
            }

            _billRepository.Remove(bill);
            await _billRepository.SaveChangesAsync();
        }

        private async Task<MonthlyTotalModel> GetOrCreateMonthlyTotalAsync(int month, int year, int userId)
        {
            var monthlyTotal = await _monthlyTotalRepository.GetMonthlyTotalByMonthAndUserAsync(month, year, userId);

            if (monthlyTotal == null)
            {
                monthlyTotal = new MonthlyTotalModel
                {
                    MonthlyTotalsYear = year,
                    MonthlyTotalsMonth = month,
                    TotalIncome = 0,
                    TotalBill = 0,
                    UserId = userId
                };
                await _monthlyTotalRepository.AddAsync(monthlyTotal);
                await _monthlyTotalRepository.SaveChangesAsync();
            }

            return monthlyTotal;
        }

        private void ValidateBillInput(decimal amount, int categoryId, DateOnly date)
        {
            if (date > DateOnly.FromDateTime(DateTime.Now.AddMonths(2)))
            {
                throw new ArgumentException("The date cannot be greater than 2 months.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("The amount of the expense must be greater than 0.");
            }
            if (categoryId == 0)
            {
                throw new ArgumentException("You must choose a category.");
            }
        }
    }
}
