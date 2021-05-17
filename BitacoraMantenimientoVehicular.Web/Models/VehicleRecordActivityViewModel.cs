using System;
using System.ComponentModel.DataAnnotations;
using BitacoraMantenimientoVehicular.Datasource.Entities;

namespace BitacoraMantenimientoVehicular.Web.Models
{
    public class VehicleRecordActivityViewModel: VehicleRecordActivityEntity
    {
        [Display(Name = "User")]
        public short ClientId { get; set; }

        [Display(Name = "Vehicle")]
        public Guid VehicleId { get; set; }
    }
}
