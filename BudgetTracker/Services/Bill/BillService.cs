using BudgetTracker.Models;
using BudgetTracker.Repositories.BillRepository;
using BudgetTracker.Repositories.CategoryRepository;
using BudgetTracker.Repositories.MonthlyTotalRepository;
using BudgetTracker.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.Services.Bill
{
    public class BillService : IBillService
    {
        private readonly IBillRepository _billRepository;
        private readonly IMonthlyTotalRepository _monthlyTotalRepository;
        private readonly ICategoryRepository _categoryRepository;

        public BillService(IBillRepository billRepository, IMonthlyTotalRepository monthlyTotalRepository, ICategoryRepository categoryRepository)
        {
            _billRepository = billRepository;
            _monthlyTotalRepository = monthlyTotalRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<BudgetTracker.Models.Bill>> GetBillsByUserAsync(int userId)
        {
            return await _billRepository.GetBillsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<BudgetTracker.Models.Bill>> GetBillsByDateAsync(int userId, DateOnly date)
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

        public async Task<BillViewModel> GetCreateViewModelAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var selectListItems = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            });

            return new BillViewModel
            {
                Bill = new BudgetTracker.Models.Bill
                {
                    BillsDate = DateOnly.FromDateTime(DateTime.Now)
                },
                Categories = selectListItems
            };
        }

        public async Task<BillViewModel> GetEditViewModelAsync(int billId, int userId, DateOnly date)
        {
            var bill = await _billRepository.GetBillByIdAndUserAsync(billId, userId);
            if (bill == null)
            {
                throw new InvalidOperationException("The bill does not exist.");
            }

            var categories = await _categoryRepository.GetAllAsync();
            var selectListItems = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            });

            return new BillViewModel
            {
                Bill = bill,
                Categories = selectListItems
            };
        }

        public async Task CreateBillAsync(int userId, int amount, int categoryId, string desc, DateOnly date)
        {
            ValidateBillInput(amount, categoryId, date);

            var bill = new BudgetTracker.Models.Bill
            {
                UserId = userId,
                BillsAmount = amount,
                BillsDesc = desc,
                BillsDate = date,
                CategoryId = categoryId
            };

            var monthlyTotal = await GetOrCreateMonthlyTotalAsync(date.Month, date.Year, userId);
            monthlyTotal.TotalBill += bill.BillsAmount;

            await _billRepository.AddAsync(bill);
            _monthlyTotalRepository.Update(monthlyTotal);
            await _billRepository.SaveChangesAsync();
        }

        public async Task UpdateBillAsync(int userId, int billId, int amount, int categoryId, string desc, DateOnly date)
        {
            ValidateBillInput(amount, categoryId, date);

            var bill = await _billRepository.GetBillByIdAndUserAsync(billId, userId);
            if (bill == null)
            {
                throw new InvalidOperationException("Bill was not found.");
            }

            var monthlyTotal = await GetOrCreateMonthlyTotalAsync(bill.BillsDate.Month, bill.BillsDate.Year, userId);
            monthlyTotal.TotalBill -= bill.BillsAmount;
            monthlyTotal.TotalBill += amount;

            bill.BillsAmount = amount;
            bill.BillsDesc = desc;
            bill.BillsDate = date;
            bill.CategoryId = categoryId;

            _monthlyTotalRepository.Update(monthlyTotal);
            _billRepository.Update(bill);
            await _billRepository.SaveChangesAsync();
        }

        public async Task DeleteBillAsync(int userId, int billId, DateOnly date)
        {
            var bill = await _billRepository.GetBillByIdAndUserAsync(billId, userId);
            if (bill == null)
            {
                throw new InvalidOperationException("The expense was not found.");
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

        private void ValidateBillInput(int amount, int categoryId, DateOnly date)
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
