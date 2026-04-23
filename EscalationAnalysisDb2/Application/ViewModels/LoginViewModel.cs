using System.ComponentModel.DataAnnotations;

namespace EscalationAnalysisDb2.Application.ViewModels
{
    public class LoginViewModel
    {
        /* [Required]
         [EmailAddress]
         public string Email { get; set; }

         [Required]
         [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
         [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
             ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
         public string Password { get; set; }*/

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}