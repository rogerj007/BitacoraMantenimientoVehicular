using System.ComponentModel.DataAnnotations;
using BitacoraMantenimientoVehicular.Datasource.Entities;

namespace BitacoraMantenimientoVehicular.Web.Models
{
    public class UserViewModel : UserEntity
    {

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[\d\w._-]+@[\d\w._-]+\.[\w]+$", ErrorMessage = "Email is invalid")]
        public override string UserName { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Vehicle Brand")]
        [Range(1, byte.MaxValue, ErrorMessage = "You must select a User Function.")]
        public byte? UserFunctionId { get; set; }

    }
}