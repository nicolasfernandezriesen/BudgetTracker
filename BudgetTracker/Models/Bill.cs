using System;
using System.Collections.Generic;

namespace BudgetTracker.Models;

public partial class Bill
{
    public int BillsId { get; set; }

    public DateOnly BillsDate { get; set; }

    public decimal BillsAmount { get; set; }

    public string? BillsDesc { get; set; }

    public int CategoryId { get; set; }

    public int UserId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
