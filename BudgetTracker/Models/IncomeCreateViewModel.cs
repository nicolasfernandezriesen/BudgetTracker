using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.Models
{
    public class IncomeCreateViewModel
    {
        public Income Income { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
