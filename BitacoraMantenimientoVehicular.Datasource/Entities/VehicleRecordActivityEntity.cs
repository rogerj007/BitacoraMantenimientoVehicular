using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitacoraMantenimientoVehicular.Datasource.Entities
{
    public class VehicleRecordActivityEntity 
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Display(Name = "Km - Hours")]
        //[Required(ErrorMessage = "The field {0} is mandatory.")]
        [Required]
        [Range(1, int.MaxValue)]
        public long KmHr { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public VehicleEntity Vehicle { get; set; }
    }
}
