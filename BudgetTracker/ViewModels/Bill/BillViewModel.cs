using BudgetTracker.ViewModels.Category;
using BillModel = BudgetTracker.Models.Bill;

namespace BudgetTracker.ViewModels.Bill
{
    public class BillViewModel
    {
        public BillModel Bill { get; set; }
        public List<CategoryGroupViewModel> AvailableCategories { get; set; } = new List<CategoryGroupViewModel>();
    }
}
