using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.ViewModels.User
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*\d).+$",
            ErrorMessage = "La contraseña debe contener al menos una minúscula y un número.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}