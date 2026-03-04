using BudgetTracker.Models;
using BudgetTracker.Repositories.CategoryRepository;
using BudgetTracker.Repositories.IncomeRepository;
using BudgetTracker.Repositories.MonthlyTotalRepository;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.Services.Income
{
    public class IncomeService : IIncomeService
    {
        private readonly IIncomeRepository _incomeRepository;
        private readonly IMonthlyTotalRepository _monthlyTotalRepository;
        private readonly ICategoryRepository _categoryRepository;

        public IncomeService(IIncomeRepository incomeRepository, IMonthlyTotalRepository monthlyTotalRepository, ICategoryRepository categoryRepository)
        {
            _incomeRepository = incomeRepository;
            _monthlyTotalRepository = monthlyTotalRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByUserAsync(int userId)
        {
            return await _incomeRepository.GetIncomeByUserIdAsync(userId);
        }

        public async Task<IEnumerable<BudgetTracker.Models.Income>> GetIncomeByDateAsync(int userId, DateOnly date)
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

        public async Task<IncomeCreateViewModel> GetCreateViewModelAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var selectListItems = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            });

            return new IncomeCreateViewModel
            {
                Income = new BudgetTracker.Models.Income
                {
                    IncomeDate = DateOnly.FromDateTime(DateTime.Now)
                },
                Categories = selectListItems
            };
        }

        public async Task CreateIncomeAsync(int userId, int amount, int categoryId, string desc, DateOnly date)
        {
            ValidateIncomeInput(amount, categoryId, date);

            var income = new BudgetTracker.Models.Income
            {
                UserId = userId,
                IncomeAmount = amount,
                IncomeDesc = desc,
                IncomeDate = date,
                CategoryId = categoryId
            };

            var monthlyTotal = await GetOrCreateMonthlyTotalAsync(date.Month, date.Year, userId);
            monthlyTotal.TotalIncome += income.IncomeAmount;

            await _incomeRepository.AddAsync(income);
            _monthlyTotalRepository.Update(monthlyTotal);
            await _incomeRepository.SaveChangesAsync();
        }

        public async Task UpdateIncomeAsync(int userId, int incomeId, int amount, int categoryId, string desc, DateOnly date)
        {
            ValidateIncomeInput(amount, categoryId, date);

            var income = await _incomeRepository.GetIncomeByIdAndUserAsync(incomeId, userId);
            if (income == null)
            {
                throw new InvalidOperationException("Income was not found.");
            }

            var monthlyTotal = await GetOrCreateMonthlyTotalAsync(income.IncomeDate.Month, income.IncomeDate.Year, userId);
            monthlyTotal.TotalIncome -= income.IncomeAmount;
            monthlyTotal.TotalIncome += amount;

            income.IncomeAmount = amount;
            income.IncomeDesc = desc;
            income.IncomeDate = date;
            income.CategoryId = categoryId;

            _monthlyTotalRepository.Update(monthlyTotal);
            _incomeRepository.Update(income);
            await _incomeRepository.SaveChangesAsync();
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

        private async Task<BudgetTracker.Models.MonthlyTotal> GetOrCreateMonthlyTotalAsync(int month, int year, int userId)
        {
            var monthlyTotal = await _monthlyTotalRepository.GetMonthlyTotalByMonthAndUserAsync(month, year, userId);

            if (monthlyTotal == null)
            {
                monthlyTotal = new BudgetTracker.Models.MonthlyTotal
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

        private void ValidateIncomeInput(int amount, int categoryId, DateOnly date)
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
