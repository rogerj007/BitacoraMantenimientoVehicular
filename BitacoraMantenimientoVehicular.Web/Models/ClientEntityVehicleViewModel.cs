using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Datasource.Entities;

namespace BitacoraMantenimientoVehicular.Web.Models
{
    public class ClientEntityVehicleViewModel: ClientEntityVehicleEntity
    {
        [Display(Name = "Client")]
        public short ClientId { get; set; }

        [Display(Name = "Vehicle")]
        public Guid VehicleId { get; set; }

        public string CreatedById { get; set; }
        public string ModifiedById { get; set; }

    }
}
