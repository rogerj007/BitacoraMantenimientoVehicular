using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using BitacoraMantenimientoVehicular.Datasource.Enums;

namespace BitacoraMantenimientoVehicular.Datasource.Entities
{
    public class ClientEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        [StringLength(70, MinimumLength = 8)]
        [MaxLength(70)]
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

        [StringLength(13, MinimumLength = 10)]
        [MaxLength(13)]
        [Required]
        public string Dni { get; set; }

        [StringLength(30)]
        [MaxLength(30)]
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Mail { get; set; }

        [StringLength(10, MinimumLength = 9)]
        [MaxLength(10)]
        [Required]
        public string Phone { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [MaxLength(10)]
        [Required]
        public string CellPhone { get; set; }

        [StringLength(25, MinimumLength = 9)]
        [MaxLength(25)]
        [Required]
        public string Telegram { get; set; }

        [StringLength(4, MinimumLength =4)]
        [MaxLength(4)]
        [Required]
        public string TelegramCode { get; set; }

        [StringLength(200, MinimumLength = 6)]
        [MaxLength(200)]
        [Required]
        public string Address { get; set; }
       
        [Display(Name = "User Type")]
        [Required]
        public UserType UserType { get; set; }

        public virtual ICollection<ClientEntityVehicleEntity> ClientEntityVehicleEntities { get; set; }

    }
}
