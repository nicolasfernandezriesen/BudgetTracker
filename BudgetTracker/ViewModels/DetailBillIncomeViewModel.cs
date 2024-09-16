using BudgetTracker.Models;

namespace BudgetTracker.ViewModels
{
    public class DetailBillIncomeViewModel
    {
        public List<Income> Income { get; set; }
        public List<Bill> Bill { get; set; }

        public DateOnly Date { get; set; }
    }
}
