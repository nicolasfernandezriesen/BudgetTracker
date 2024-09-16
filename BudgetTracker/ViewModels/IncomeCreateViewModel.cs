using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.ViewModels
{
    public class IncomeCreateViewModel
    {
        public Income Income { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
