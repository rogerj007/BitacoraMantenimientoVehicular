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

        [Display(Name = "Km")]
        [Required]
        [Range(1, long.MaxValue)]
        public long Km { get; set; }

        public decimal? Longitud { get; set; }
        public decimal? Latitud { get; set; }


        [Column(TypeName = "datetimeoffset")]
        [DataType(DataType.DateTime)]
       public DateTimeOffset CreatedDate { get; set; }

        [Column(TypeName = "datetimeoffset")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset? ModifiedDate { get; set; }
        public ClientEntity RegisterBy { get; set; }
        public virtual VehicleEntity Vehicle { get; set; }
    }
}
