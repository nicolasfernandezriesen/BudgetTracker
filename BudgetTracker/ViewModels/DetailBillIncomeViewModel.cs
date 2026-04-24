using BudgetTracker.Models;
using BillModel = BudgetTracker.Models.Bill;
using IncomeModel = BudgetTracker.Models.Income;

namespace BudgetTracker.ViewModels
{
    public class DetailBillIncomeViewModel
    {
        public List<IncomeModel> Income { get; set; }
        public List<BillModel> Bill { get; set; }

        public DateOnly Date { get; set; }
    }
}
