using System.Threading.Tasks;
using AutoMapper;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using BitacoraMantenimientoVehicular.Web.Models;

namespace BitacoraMantenimientoVehicular.Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public ConverterHelper(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<VehicleEntity> ToVehicleAsync(VehicleViewModel model, string path)
        {
            var dto = _mapper.Map<VehicleEntity>(model);
            dto.VehicleBrand = await _context.VehicleBrand.FindAsync(model.VehicleBrandId);
            dto.VehicleStatus = await _context.VehicleStatus.FindAsync(model.VehicleStatusId);
            dto.Country = await _context.Country.FindAsync(model.CountryId);
            dto.Fuel = await _context.Fuel.FindAsync(model.FuelId);
            dto.Color = await _context.Color.FindAsync(model.ColorId);
            if (!string.IsNullOrEmpty(path)) dto.ImageUrl = path;
            return dto;
        }
       
    }

}
