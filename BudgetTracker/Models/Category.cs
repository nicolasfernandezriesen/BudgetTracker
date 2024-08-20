using System;
using System.Collections.Generic;

namespace BudgetTracker.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();
}
