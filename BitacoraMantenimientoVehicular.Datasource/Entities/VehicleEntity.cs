using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitacoraMantenimientoVehicular.Datasource.Entities
{
    public class VehicleEntity
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [RegularExpression(@"^([A-Za-z]{3}\d{4})$")]
        [Column("Plaque")]
        [Required]
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


       

        [StringLength(25, MinimumLength = 4)]
        [MaxLength(25)]
        [Required]
        public string MotorSerial  { get; set; }

        [StringLength(25, MinimumLength = 4)]
        [Required]
        public string Vin { get; set; }

        [Required]
        [Range(1000, 15000, ErrorMessage = "Year must be from 1000")]
        public int Cylinder { get; set; }
        
        [Display(Name = "Release Date")]
        [Required]
        [Range(1900, int.MaxValue, ErrorMessage = "Year must be from 2000")]
        public short Year { get; set; }

        [Display(Name = "Km - Hr Actual")]
        public long KmHrActual { get; set; }

        [Display(Name = "Image")]
        public string ImageUrl { get; set; }
     
        public VehicleBrandEntity VehicleBrand { get; set; }
      
        public CountryEntity Country { get; set; }
        public FuelEntity Fuel { get; set; }
        public ColorEntity Color { get; set; }
        public VehicleStatusEntity VehicleStatus { get; set; }

        public ICollection<VehicleRecordActivityEntity> VehicleRecordActivities { get; set; }

    }
}
