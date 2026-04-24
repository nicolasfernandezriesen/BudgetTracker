using BudgetTracker.Models;
using BillModel = BudgetTracker.Models.Bill;
using IncomeModel = BudgetTracker.Models.Income;

namespace BudgetTracker.ViewModels
{
    public class HistoryViewModel
    {
        public List<IncomeModel> Income { get; set; }
        public List<BillModel> Bill { get; set; }
        public MonthlyTotal MonthlyTotal { get; set; }
    }
}
