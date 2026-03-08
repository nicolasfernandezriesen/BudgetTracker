using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.ViewModels.User
{
    public class CreateViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no es valido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*\d).+$",
        ErrorMessage = "La contraseña debe contener al menos una minúscula y un número.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de la contraseña es obligatoria.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}