using AutoMapper;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using BitacoraMantenimientoVehicular.Web.Models;

namespace BitacoraMantenimientoVehicular.Web.Helpers
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {

            CreateMap<VehicleEntity, VehicleViewModel>().ReverseMap();
            CreateMap<UserEntity, UserViewModel>().ReverseMap();
            //.ForMember(dest => dest.VehicleBrandId,
            //    opt => opt.MapFrom(src => src.VehicleBrand.Id))
            //.ForMember(dest => dest.ColorId,
            //    opt => opt.MapFrom(src => src.Color.Id))
            //.ForMember(dest => dest.Fuel,
            //    opt => opt.MapFrom(src => src.Fuel.Id))
            //.ForMember(dest => dest.VehicleTypeId,
            //    opt => opt.MapFrom(src => src.VehicleType.Id))
            //.ForMember(dest => dest.VehicleStatusId,
            //    opt => opt.MapFrom(src => src.VehicleStatus.Id))
            //.ForMember(dest => dest.CountryId,
            //    opt => opt.MapFrom(src => src.Country.Id))





            //CreateMap<VehicleRecordActivityEntity, VehicleRecordActivityViewModel>().ReverseMap();

            //CreateMap<VehicleMaintenanceEntity, VehicleMaintenanceViewModel>().ReverseMap();

            //CreateMap<VehicleMaintenanceDetailEntity, VehicleMaintenanceDetailsViewModel>().ReverseMap();

            //CreateMap<UserEntity, EditUserViewModel>().ReverseMap();

        }
    }
}
