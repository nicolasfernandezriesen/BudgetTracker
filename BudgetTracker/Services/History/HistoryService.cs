using BudgetTracker.Repositories.Bill;
using BudgetTracker.Repositories.Income;
using BudgetTracker.Repositories.MonthlyTotal;
using BudgetTracker.ViewModels;

namespace BudgetTracker.Services.History
{
    public class HistoryService : IHistoryService
    {
        private readonly IIncomeRepository _incomeRepository;
        private readonly IBillRepository _billRepository;
        private readonly IMonthlyTotalRepository _monthlyTotalRepository;

        public HistoryService(IIncomeRepository incomeRepository, IBillRepository billRepository, IMonthlyTotalRepository monthlyTotalRepository)
        {
            _incomeRepository = incomeRepository;
            _billRepository = billRepository;
            _monthlyTotalRepository = monthlyTotalRepository;
        }

        public async Task<HistoryViewModel> GetBillIncomeAndMonthlyTotalAsync(int userId, int month, int year)
        {
            var income = await _incomeRepository.GetIncomeByMonthAsync(userId, month, year);
            var bills = await _billRepository.GetBillsByMonthAsync(userId, month, year);
            var monthlyTotal = await _monthlyTotalRepository.GetMonthlyTotalByMonthAndUserAsync(month, year, userId);

            // Crear MonthlyTotal vacío si no existe
            if (monthlyTotal == null)
            {
                monthlyTotal = new global::BudgetTracker.Models.MonthlyTotal
                {
                    MonthlyTotalsMonth = month,
                    MonthlyTotalsYear = year,
                    UserId = userId
                };
            }

            return new HistoryViewModel
            {
                Income = income.ToList(),
                Bill = bills.ToList(),
                MonthlyTotal = monthlyTotal
            };
        }
    }
}
