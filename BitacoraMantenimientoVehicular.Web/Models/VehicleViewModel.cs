using System.ComponentModel.DataAnnotations;
using BitacoraMantenimientoVehicular.Datasource.Entities;

namespace BitacoraMantenimientoVehicular.Web.Models
{
    public class VehicleViewModel : VehicleEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Vehicle Brand")]
        [Range(1, byte.MaxValue, ErrorMessage = "You must select a Vehicle Brand.")]
        public short VehicleBrandId { get; set; }
   

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Vehicle Status")]
        [Range(1, byte.MaxValue, ErrorMessage = "You must select a Vehicle Status.")]
        public byte VehicleStatusId { get; set; }


        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Country")]
        [Range(1, byte.MaxValue, ErrorMessage = "You must select a Country.")]
        public short CountryId { get; set; }


        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Fuel")]
        [Range(1, byte.MaxValue, ErrorMessage = "You must select a Fuel.")]
        public byte FuelId { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Color")]
        [Range(1, byte.MaxValue, ErrorMessage = "You must select a Color.")]
        public short ColorId { get; set; }



        //[Display(Name = "Image")]
        //public IFormFile ImageFile { get; set; }
        //public IEnumerable<SelectListItem> VehicleBrands { get; set; }
        //public IEnumerable<SelectListItem> VehicleTypes { get; set; }
        //public IEnumerable<SelectListItem> VehicleStatu { get; set; }
        //public IEnumerable<SelectListItem> Countries { get; set; }
        //public IEnumerable<SelectListItem> Fuels { get; set; }
        //public IEnumerable<SelectListItem> Colors { get; set; }

    }
}
