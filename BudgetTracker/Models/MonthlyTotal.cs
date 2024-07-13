using System;
using System.Collections.Generic;

namespace BudgetTracker.Models;

public partial class MonthlyTotal
{
    public int MonthlyTotalsId { get; set; }

    public int MonthlyTotalsYear { get; set; }

    public int MonthlyTotalsMonth { get; set; }

    public decimal? TotalIncome { get; set; }

    public decimal? TotalBill { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
