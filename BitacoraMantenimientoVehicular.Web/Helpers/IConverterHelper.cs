using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using BitacoraMantenimientoVehicular.Web.Models;

namespace BitacoraMantenimientoVehicular.Web.Helpers
{
    public interface IConverterHelper
    {
        Task<VehicleEntity> ToVehicleAsync(VehicleViewModel model, string path);
       
    }
}