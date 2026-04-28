using BudgetTracker.ViewModels.Category;
using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.ViewModels.Bill
{
    public class CreateViewModel
    {
        [Required(ErrorMessage = "La fecha del gasto es obligatoria.")]
        [Display(Name = "Fecha del gasto")]
        public DateOnly BillsDate { get; set; }

        [Required(ErrorMessage = "El monto del gasto es obligatorio.")]
        [Display(Name = "Monto del gasto")]
        public decimal BillsAmount { get; set; }

        [Display(Name = "Descripción (opcional)")]
        public string? BillsDesc { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }

        public List<CategoryGroupViewModel> AvailableCategories { get; set; } = new List<CategoryGroupViewModel>();
    }
}
