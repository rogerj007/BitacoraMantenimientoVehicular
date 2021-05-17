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

        [Display(Name = "Km ")]
        [Required]
        [Range(1, long.MaxValue)]
        public long Km { get; set; }

        [Column(TypeName = "datetime2")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
        public DateTime CreatedDateLocal => CreatedDate.ToLocalTime();

        [Column(TypeName = "datetime2")]
        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }
        public DateTime? ModifiedDateLocal => ModifiedDate?.ToLocalTime();
        public ClientEntity RegisterBy { get; set; }
        public virtual VehicleEntity Vehicle { get; set; }
    }
}
