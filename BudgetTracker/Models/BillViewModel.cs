using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.Models
{
    public class BillViewModel
    {
        public Bill Bill { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
