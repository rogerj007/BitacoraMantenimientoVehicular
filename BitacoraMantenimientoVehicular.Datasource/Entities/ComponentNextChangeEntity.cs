using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitacoraMantenimientoVehicular.Datasource.Entities
{
    public class ComponentNextChangeEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column(TypeName = "datetimeoffset")]
        [DataType(DataType.DateTime)]
       public DateTimeOffset CreatedDate { get; set; }

        [Column(TypeName = "datetimeoffset")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset? ModifiedDate { get; set; }


        [Display(Name = "Km")]
        [Required]
        [Range(1, long.MaxValue)]
        public long Km { get; set; }

        public bool IsComplete { get; set; }

        public virtual VehicleEntity Vehicle { get; set; }

        public ComponentEntity Component { get; set; }


    }
}
