using System;
using System.Linq;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using BitacoraMantenimientoVehicular.Datasource.Enums;
using BitacoraMantenimientoVehicular.Web.Helpers;

namespace BitacoraMantenimientoVehicular.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext dataContext, IUserHelper userHelper)//,
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            try
            {
                await _dataContext.Database.EnsureCreatedAsync();

                //Roles
              //  await CheckRolesAsync();
                await CheckUsersAsync();


                //Variables
             
                await CheckCountryAsync().ConfigureAwait(false);
                await CheckComponetsAsync().ConfigureAwait(false);
                await CheckColorAsync().ConfigureAwait(false);
                await CheckFuelAsync().ConfigureAwait(false);
                await CheckVehiculeBrandAsync().ConfigureAwait(false); 
                await CheckVehiculeStatusAsync().ConfigureAwait(false);
                //Create Events
                 await CheckVehicleAsync().ConfigureAwait(false);
                 await CheckClientAsync().ConfigureAwait(false);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
           

        }




        //private async Task CheckRolesAsync()
        //{
        //    await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
        //    await _userHelper.CheckRoleAsync(UserType.Supervisor.ToString());
        //    await _userHelper.CheckRoleAsync(UserType.User.ToString());
        //}

        private async Task CheckUsersAsync()
        {
            await CheckUserAsync("1010", "Admin", "Web", "ajelonarvaez.m92@gmail.com", "00000000", "Calle Luna Calle Venus", true);
        }

        private async Task<UserEntity> CheckUserAsync(
            string document,
            string firstName,
            string lastName,
            string email,
            string phone,
            string address,
            bool enable
            )
        {
            var user = await _userHelper.GetUserByEmailAsync(email);
            if (user != null) return user;
            user = new UserEntity
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                PhoneNumber = phone,
                Address = address,
                Document = document,
                IsEnable =enable
              
            };
            await _userHelper.AddUserAsync(user, "123456");
            return user;
        }


        #region Variables

        private async Task CheckComponetsAsync()
        {
            if (!_dataContext.Component.Any())
            {
                await _dataContext.Component.AddRangeAsync(
                    new ComponentEntity { Name = "FILTRO TRAMPA DE AGUA",Code="xxxx",Ttl=5000, CreatedDate = DateTime.UtcNow,IsEnable=true },
                    new ComponentEntity { Name = "FILTRO COMBUSTIBLE", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new ComponentEntity { Name = "FILTRO DE ACEITE", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new ComponentEntity { Name = "FILTRO ACEITE HIDRAULICO", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new ComponentEntity { Name = "FILTRO DE AIRE", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new ComponentEntity { Name = "FILTRO DE AIRE SECUNDARIO", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new ComponentEntity { Name = "FILTRO COMBUSTIBLE PRIMARIO", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new ComponentEntity { Name = "FILTRO COMBUSTIBLE SECUNDARIO", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new ComponentEntity { Name = "FILTRO RACOR", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new ComponentEntity { Name = "FILTRO SECADOR", Code = "xxxx", Ttl = 5000, CreatedDate = DateTime.UtcNow, IsEnable = true }

                );
                await _dataContext.SaveChangesAsync();
            }
        }
        private async Task CheckVehiculeBrandAsync()
        {
            if (!_dataContext.VehicleBrand.Any())
            {
                await _dataContext.VehicleBrand.AddRangeAsync(
                    new VehicleBrandEntity { Name = "CHEVROLET", CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new VehicleBrandEntity { Name = "KENWORTH", CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new VehicleBrandEntity { Name = "HINO MOTORS", CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new VehicleBrandEntity { Name = "TOYOTA", CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new VehicleBrandEntity { Name = "MAZDA", CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new VehicleBrandEntity { Name = "CATERPILLAR", CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new VehicleBrandEntity { Name = "KOMATSU", CreatedDate = DateTime.UtcNow, IsEnable = true },
                    new VehicleBrandEntity { Name = "JOHN DEERE", CreatedDate = DateTime.UtcNow, IsEnable = true }

                );
                await _dataContext.SaveChangesAsync();
            }
        }

        private async Task CheckVehiculeStatusAsync()
        {
            try
            {
                if (!_dataContext.VehicleStatus.Any())
                {
                    await _dataContext.VehicleStatus.AddRangeAsync(
                        new VehicleStatusEntity { Name = "OPTIMO", CreatedDate = DateTime.UtcNow, IsEnable = true },
                        new VehicleStatusEntity { Name = "FUNCIONAL", CreatedDate = DateTime.UtcNow, IsEnable = true},
                        new VehicleStatusEntity { Name = "PESIMO", CreatedDate = DateTime.UtcNow, IsEnable = true }
                    );
                    await _dataContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private async Task CheckVehicleAsync()
        {
            try
            {
                if (!_dataContext.Vehicle.Any())
                {
                    var colorVehicle1 = _dataContext.Color.Single(c => c.Id.Equals(1));
                    var colorVehicle2 = _dataContext.Color.Single(c => c.Id.Equals(3));
                    var countryVehicle1 = _dataContext.Country.Single(c => c.Id.Equals(1));
                    var countryVehicle2 = _dataContext.Country.Single(c => c.Id.Equals(3));
                    var vehicleBrandVehicle1 = _dataContext.VehicleBrand.Single(c => c.Id.Equals(1));
                    var vehicleBrandVehicle2 = _dataContext.VehicleBrand.Single(c => c.Id.Equals(3));
                    var vehicleFuelVehicle1 = _dataContext.Fuel.Single(c => c.Id.Equals(1));
                    var vehicleFuelVehicle2 = _dataContext.Fuel.Single(c => c.Id.Equals(3));
                    var vehicleStatusVehicle1 = _dataContext.VehicleStatus.Single(c => c.Id.Equals(1));
                    var vehicleStatusVehicle2 = _dataContext.VehicleStatus.Single(c => c.Id.Equals(2));

                    await _dataContext.Vehicle.AddRangeAsync(
                        new VehicleEntity { Name = "PBX1234",
                                            Vin = "123456",
                                            KmRegistro=25500,
                                            KmActual=25500,
                                            Cylinder = 1500,
                                            MotorSerial = "159XXXX",   
                                            Year=2021,
                                            CreatedDate = DateTime.UtcNow,
                                            IsEnable = true, 
                                            Color = colorVehicle1,
                                            Country= countryVehicle1,
                                            VehicleBrand= vehicleBrandVehicle1,
                                            Fuel= vehicleFuelVehicle1,
                                           VehicleStatus= vehicleStatusVehicle1

                        },
                        new VehicleEntity { Name = "GYE6547",
                                            Vin = "654321",
                                            Cylinder = 30500,
                                            KmRegistro = 2750,
                                            KmActual=2750,
                                            MotorSerial="159EDFFF",
                                            Year=2011,
                                            CreatedDate = DateTime.UtcNow, 
                                            IsEnable = true,
                                            Color = colorVehicle2,
                                            Country = countryVehicle2,
                                            VehicleBrand = vehicleBrandVehicle2,
                                            Fuel = vehicleFuelVehicle2,
                                            VehicleStatus= vehicleStatusVehicle2
                        }

                    ); 
                    await _dataContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private async Task CheckClientAsync()
        {
            try
            {
                if (!_dataContext.Client.Any())
                {
                    await _dataContext.Client.AddRangeAsync(
                        new ClientEntity
                        {
                            Name = "ROGER JAIMES",
                            CreatedDate = DateTime.UtcNow,
                            IsEnable = true,
                            Dni="1715150182",
                            Mail="dnranjo@yopmail.com",
                            CellPhone="0898575844",
                            Phone="0324562600",
                            Telegram= "135198016",
                            TelegramCode = "1234",
                            Address="Cuenca-Ecuador",
                            UserType=UserType.Owner
                        },
                        new ClientEntity
                        {
                            Name = "CARLOS ESPINOZA",
                            CreatedDate = DateTime.UtcNow,
                            IsEnable = true,
                            Dni = "98765432100",
                            Mail = "cespinoza@yopmail.com",
                            CellPhone = "0994575840",
                            Phone = "0224162600",
                            Telegram = "0000000000",
                            TelegramCode="1234",
                            Address = "Quito-Ecuador",
                            UserType = UserType.Driver
                        },
                        new ClientEntity
                        {
                            Name = "MAURICIO ESPIN",
                            CreatedDate = DateTime.UtcNow,
                            IsEnable = true,
                            Dni = "9875500",
                            Mail = "mespin@yopmail.com",
                            CellPhone = "0963575840",
                            Phone = "0224162600",
                            Telegram = "0000000000",
                            TelegramCode = "1234",
                            Address = "Mata-Ecuador",
                            UserType = UserType.Collector
                        }
                        , new ClientEntity
                        {
                            Name = "ELENA GALARRAGA",
                            CreatedDate = DateTime.UtcNow,
                            IsEnable = true,
                            Dni = "98456432100",
                            Mail = "egalarraga@yopmail.com",
                            CellPhone = "0904575840",
                            Phone = "0224162600",
                            Telegram = "0000000000",
                            TelegramCode = "1234",
                            Address = "Ibarra-Ecuador",
                            UserType = UserType.Collector
                        }

                    );
                    await _dataContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
       

        private async Task CheckFuelAsync()
        {
            if (!_dataContext.Fuel.Any())
            {
                await _dataContext.Fuel.AddRangeAsync(
                    new FuelEntity { Name = "EXTRA", CreatedDate = DateTime.UtcNow },
                    new FuelEntity { Name = "SUPER", CreatedDate = DateTime.UtcNow },
                    new FuelEntity { Name = "ECOPAIS", CreatedDate = DateTime.UtcNow },
                    new FuelEntity { Name = "DIESEL", CreatedDate = DateTime.UtcNow }
                );
                await _dataContext.SaveChangesAsync();

            }
        }
        private async Task CheckColorAsync()
        {
            if (!_dataContext.Color.Any())
            {
                await _dataContext.Color.AddRangeAsync(
                    new ColorEntity { Name = "ROJO", CreatedDate = DateTime.UtcNow },
                                new ColorEntity { Name = "NEGRO", CreatedDate = DateTime.UtcNow },
                                new ColorEntity { Name = "AZUL", CreatedDate = DateTime.UtcNow },
                                new ColorEntity { Name = "AMARILLO", CreatedDate = DateTime.UtcNow },
                                new ColorEntity { Name = "NARANJA", CreatedDate = DateTime.UtcNow },
                                new ColorEntity { Name = "BLANCO", CreatedDate = DateTime.UtcNow },
                                new ColorEntity { Name = "MORADO", CreatedDate = DateTime.UtcNow },
                                new ColorEntity { Name = "PLOMO", CreatedDate = DateTime.UtcNow },
                                new ColorEntity { Name = "MARRON", CreatedDate = DateTime.UtcNow }
                    );
                await _dataContext.SaveChangesAsync();

            }
        }

        private async Task CheckCountryAsync()
        {
            if (!_dataContext.Country.Any())
            {
                await _dataContext.Country.AddRangeAsync(
                    new CountryEntity { Name = "ECUADOR", CreatedDate = DateTime.UtcNow },
                    new CountryEntity { Name = "COLOMBIA", CreatedDate = DateTime.UtcNow },
                    new CountryEntity { Name = "CHINA", CreatedDate = DateTime.UtcNow },
                    new CountryEntity { Name = "JAPON", CreatedDate = DateTime.UtcNow },
                    new CountryEntity { Name = "TAILANDIA", CreatedDate = DateTime.UtcNow },
                    new CountryEntity { Name = "U.S.A", CreatedDate = DateTime.UtcNow }
                );
                await _dataContext.SaveChangesAsync();

            }
        }

        

        #endregion

    }
}
