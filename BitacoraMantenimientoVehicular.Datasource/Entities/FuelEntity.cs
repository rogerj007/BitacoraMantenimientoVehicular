using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitacoraMantenimientoVehicular.Datasource.Entities
{
    public class FuelEntity
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; }

        [Display(Name = "Fuel")]
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [MaxLength(50)]
        public string Name { get; set; }

        [Column(TypeName = "datetime2")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
        public DateTime CreatedDateLocal => CreatedDate.ToLocalTime();

        [Column(TypeName = "datetime2")]
        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }
        public DateTime? ModifiedDateLocal => ModifiedDate?.ToLocalTime();
        public UserEntity CreatedBy { get; set; }
        public UserEntity ModifiedBy { get; set; }

        public bool IsEnable { get; set; }


        public ICollection<VehicleEntity> Vehicles { get; set; }

    }
}
