using BudgetTracker.ViewModels.Category;
using Microsoft.AspNetCore.Mvc.Rendering;
using IncomeModel = BudgetTracker.Models.Income;

namespace BudgetTracker.ViewModels.Income
{
    public class IncomeCreateViewModel
    {
        public IncomeModel Income { get; set; }
        public List<CategoryGroupViewModel> AvailableCategories { get; set; } = new List<CategoryGroupViewModel>();
    }
}
