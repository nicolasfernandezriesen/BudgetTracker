using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int? parentcategoryid { get; set; }

    [ForeignKey("parentcategoryid")]
    public Category? ParentCategory { get; set; }

    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();
}