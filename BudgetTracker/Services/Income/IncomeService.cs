using BudgetTracker.Repositories.CategoryRepository;
using BudgetTracker.Repositories.IncomeRepository;
using BudgetTracker.Repositories.MonthlyTotalRepository;
using IncomeModel = BudgetTracker.Models.Income;
using MonthlyTotalModel = BudgetTracker.Models.MonthlyTotal;
using BudgetTracker.ViewModels;
using BudgetTracker.ViewModels.Category;
using BudgetTracker.ViewModels.Income;

namespace BudgetTracker.Services.Income
{
    public class IncomeService : IIncomeService
    {
        private readonly IIncomeRepository _incomeRepository;
        private readonly IMonthlyTotalRepository _monthlyTotalRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<IncomeService> _logger;

        public IncomeService(IIncomeRepository incomeRepository, IMonthlyTotalRepository monthlyTotalRepository, ICategoryRepository categoryRepository, ILogger<IncomeService> logger)
        {
            _incomeRepository = incomeRepository;
            _monthlyTotalRepository = monthlyTotalRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<IncomeModel>> GetIncomeByUserAsync(int userId)
        {
            return await _incomeRepository.GetIncomeByUserIdAsync(userId);
        }

        public async Task<IEnumerable<IncomeModel>> GetIncomeByDateAsync(int userId, DateOnly date)
        {
            return await _incomeRepository.GetIncomeByUserAndDateAsync(userId, date);
        }

        public async Task<DetailBillIncomeViewModel> GetIncomeDetailsAsync(int userId, DateOnly date)
        {
            var incomes = await _incomeRepository.GetIncomeByUserAndDateAsync(userId, date);
            
            if (!incomes.Any())
            {
                throw new InvalidOperationException("The income/s does not exist.");
            }

            return new DetailBillIncomeViewModel
            {
                Income = incomes.ToList(),
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
                IncomeDate = DateOnly.FromDateTime(DateTime.Now),
                AvailableCategories = groupedCategories
            };
        }

        public async Task CreateIncomeAsync(int userId, CreateViewModel viewmodel)
        {
            _logger.LogInformation("Inicio de CreateIncomeAsync. UserId: {UserId}", userId);

            try
            {
                ValidateIncomeInput(viewmodel.IncomeAmount, viewmodel.CategoryId, viewmodel.IncomeDate);

                var income = new IncomeModel
                {
                    UserId = userId,
                    IncomeAmount = viewmodel.IncomeAmount,
                    IncomeDesc = viewmodel.IncomeDesc,
                    IncomeDate = viewmodel.IncomeDate,
                    CategoryId = viewmodel.CategoryId
                };

                var monthlyTotal = await GetOrCreateMonthlyTotalAsync(viewmodel.IncomeDate.Month, viewmodel.IncomeDate.Year, userId);
                monthlyTotal.TotalIncome += income.IncomeAmount;

                await _incomeRepository.AddAsync(income);
                _monthlyTotalRepository.Update(monthlyTotal);
                await _incomeRepository.SaveChangesAsync();

                _logger.LogInformation("CreateIncomeAsync completado. UserId: {UserId}. Amount: {Amount}. CategoryId: {CategoryId}", userId, viewmodel.IncomeAmount, viewmodel.CategoryId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "CreateIncomeAsync rechazado por validación. UserId: {UserId}", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateIncomeAsync. UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<EditViewModel> GetEditViewModelAsync(int userId, int incomeId)
        {
            var income = await _incomeRepository.GetIncomeByIdAndUserAsync(incomeId, userId);
            if (income == null)
            {
                throw new InvalidOperationException("The income was not found.");
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
                IncomeId = income.IncomeId,
                IncomeDate = income.IncomeDate,
                CategoryId = income.CategoryId,
                IncomeAmount = income.IncomeAmount,
                IncomeDesc = income.IncomeDesc,
                AvailableCategories = groupedCategories
            };
        }

        public async Task UpdateIncomeAsync(int userId, int incomeId, EditViewModel viewModel)
        {
            _logger.LogInformation("Inicio de UpdateIncomeAsync. UserId: {UserId}. IncomeId: {IncomeId}", userId, incomeId);

            try
            {
                ValidateIncomeInput(viewModel.IncomeAmount, viewModel.CategoryId, viewModel.IncomeDate);

                var income = await _incomeRepository.GetIncomeByIdAndUserAsync(incomeId, userId);
                if (income == null)
                {
                    _logger.LogWarning("UpdateIncomeAsync falló: ingreso no encontrado. UserId: {UserId}. IncomeId: {IncomeId}", userId, incomeId);
                    throw new InvalidOperationException("Income was not found.");
                }

                var monthlyTotal = await GetOrCreateMonthlyTotalAsync(income.IncomeDate.Month, income.IncomeDate.Year, userId);
                monthlyTotal.TotalIncome -= income.IncomeAmount;
                monthlyTotal.TotalIncome += viewModel.IncomeAmount;

                income.IncomeAmount = viewModel.IncomeAmount;
                income.IncomeDesc = viewModel.IncomeDesc;
                income.IncomeDate = viewModel.IncomeDate;
                income.CategoryId = viewModel.CategoryId;

                _monthlyTotalRepository.Update(monthlyTotal);
                _incomeRepository.Update(income);
                await _incomeRepository.SaveChangesAsync();

                _logger.LogInformation("UpdateIncomeAsync completado. UserId: {UserId}. IncomeId: {IncomeId}. Amount: {Amount}. CategoryId: {CategoryId}", userId, incomeId, viewModel.IncomeAmount, viewModel.CategoryId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "UpdateIncomeAsync rechazado por validación. UserId: {UserId}. IncomeId: {IncomeId}", userId, incomeId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en UpdateIncomeAsync. UserId: {UserId}. IncomeId: {IncomeId}", userId, incomeId);
                throw;
            }
        }

        public async Task DeleteIncomeAsync(int userId, int incomeId, DateOnly date)
        {
            var income = await _incomeRepository.GetIncomeByIdAndUserAsync(incomeId, userId);
            if (income == null)
            {
                throw new InvalidOperationException("The income was not found.");
            }

            var monthlyTotal = await _monthlyTotalRepository.GetMonthlyTotalByMonthAndUserAsync(
                income.IncomeDate.Month, income.IncomeDate.Year, userId);

            if (monthlyTotal != null)
            {
                monthlyTotal.TotalIncome -= income.IncomeAmount;
                _monthlyTotalRepository.Update(monthlyTotal);
            }

            _incomeRepository.Remove(income);
            await _incomeRepository.SaveChangesAsync();
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

        private void ValidateIncomeInput(decimal amount, int categoryId, DateOnly date)
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
    }
}
