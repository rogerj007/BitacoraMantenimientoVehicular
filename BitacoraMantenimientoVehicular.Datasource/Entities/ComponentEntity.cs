using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitacoraMantenimientoVehicular.Datasource.Entities
{
    public class ComponentEntity
    {



        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short Id { get; set; }

        [Display(Name = "Component")]
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [MaxLength(50)]
        public string Name { get; set; }


        [Required]
        [StringLength(25, MinimumLength = 4)]
        [MaxLength(25)]
        public string Code { get; set; }

        [Required]
        public long Ttl { get; set; }

        [Column(TypeName = "datetimeoffset")]
        [DataType(DataType.DateTime)]
       public DateTimeOffset CreatedDate { get; set; }

        [Column(TypeName = "datetimeoffset")]
        [DataType(DataType.DateTime)]
        public DateTimeOffset? ModifiedDate { get; set; }
        public UserEntity CreatedBy { get; set; }
        public UserEntity ModifiedBy { get; set; }

        public bool IsEnable { get; set; }

    }
}
