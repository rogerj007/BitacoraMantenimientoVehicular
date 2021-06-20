﻿using System;
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

        [Column(TypeName = "datetimeoffset")]
        [DataType(DataType.DateTime)]
       public DateTimeOffset CreatedDate { get; set; }

        [Column(TypeName = "datetimeoffset")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset? ModifiedDate { get; set; }
        public UserEntity CreatedBy { get; set; }
        public UserEntity ModifiedBy { get; set; }

        public bool IsEnable { get; set; }


        public ICollection<VehicleEntity> Vehicles { get; set; }

    }
}
