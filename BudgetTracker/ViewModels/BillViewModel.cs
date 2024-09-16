using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.ViewModels
{
    public class BillViewModel
    {
        public Bill Bill { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
