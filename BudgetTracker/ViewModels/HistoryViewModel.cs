using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetTracker.ViewModels
{
    public class HistoryViewModel
    {
        public List<Income> Income { get; set; }
        public List<Bill> Bill { get; set; }
        public MonthlyTotal MonthlyTotal { get; set; }
    }
}
