using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.ViewModels.User
{
    public class LoginViewModel
    {
        [Required (ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress]
        [Display(Name = "Correo electrónico")]
        public required string Email { get; set; }

        [Required (ErrorMessage = "El campo Contraseña es obligatorio.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public required string Password { get; set; }
    }
}