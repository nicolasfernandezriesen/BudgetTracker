using Microsoft.AspNetCore.Identity;

namespace BudgetTracker.Models;

public partial class User : IdentityUser<int>
{
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    public virtual ICollection<MonthlyTotal> MonthlyTotals { get; set; } = new List<MonthlyTotal>();
}
