using Categories = BudgetTracker.Models.Category;

namespace BudgetTracker.ViewModels.Category
{
    public class CategoryGroupViewModel
    {
        public string GroupName { get; set; } = string.Empty;

        public List<Categories> SubCategories { get; set; } = new List<Categories>();
    }
}
