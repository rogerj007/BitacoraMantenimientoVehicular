using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitacoraMantenimientoVehicular.Datasource.Entities
{
    public class RecordNotificationEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public bool Mail { get; set; }

        public bool Telegram { get; set; }

        public virtual ClientEntity Client { get; set; }

        public virtual VehicleEntity Vehicle { get; set; }

        public virtual VehicleRecordActivityEntity VehicleRecordActivity { get; set; }

        [Column(TypeName = "datetime2")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
        public DateTime CreatedDateLocal => CreatedDate.ToLocalTime();
    }
}