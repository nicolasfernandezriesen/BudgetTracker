using System;
using System.Collections.Generic;

namespace BudgetTracker.Models;

public partial class Income
{
    public int IncomeId { get; set; }

    public DateOnly IncomeDate { get; set; }

    public decimal IncomeAmount { get; set; }

    public string? IncomeDesc { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
