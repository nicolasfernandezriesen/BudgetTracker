using System;
using System.Collections.Generic;

namespace BudgetTracker.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    public virtual ICollection<MonthlyTotal> MonthlyTotals { get; set; } = new List<MonthlyTotal>();
}
