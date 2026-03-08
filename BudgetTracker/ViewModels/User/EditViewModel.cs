using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.ViewModels.User
{
    public class EditViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no es valido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        [Display(Name = "Confirmar Contraseña")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Contraseña Actual")]
        public string? OldPassword { get; set; }
    }
}
