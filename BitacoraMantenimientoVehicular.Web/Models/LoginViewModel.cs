using System.ComponentModel.DataAnnotations;

namespace BitacoraMantenimientoVehicular.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "User Name")]
        public string Username { get; set; }

        [Required]
        [MinLength(3)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

}
