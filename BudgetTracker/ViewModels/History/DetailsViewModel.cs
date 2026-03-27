namespace BudgetTracker.ViewModels.History
{
    public class DetailsViewModel
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public decimal TotalIncome { get; set; }

        public decimal TotalBill { get; set; }

        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<CategoryIncomeDto> CategoriesIncomes { get; set; } = new();
        public List<CategoryBillDto> CategoriesBills { get; set; } = new();
    }

    public class Transaction
    {
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Ingreso,
        Gasto
    }

    public class CategoryIncomeDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
    }

    public class CategoryBillDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalBill { get; set; }
    }
}
