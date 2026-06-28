using BudgetTracker.ViewModels.Category;
using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.ViewModels.Income
{
    public class EditViewModel
    {
        public int IncomeId { get; set; }

        [Required(ErrorMessage = "La fecha del ingreso es obligatoria.")]
        [Display(Name = "Fecha del ingreso")]
        public DateOnly IncomeDate { get; set; }

        [Required(ErrorMessage = "El monto del ingreso es obligatorio.")]
        [Display(Name = "Monto del ingreso")]
        public decimal IncomeAmount { get; set; }

        [Display(Name = "Descripción (opcional)")]
        public string? IncomeDesc { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }

        public List<CategoryGroupViewModel> AvailableCategories { get; set; } = new List<CategoryGroupViewModel>();
    }
}
