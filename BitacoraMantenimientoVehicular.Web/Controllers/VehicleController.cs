using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using BitacoraMantenimientoVehicular.Web.Helpers;
using BitacoraMantenimientoVehicular.Web.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BitacoraMantenimientoVehicular.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class VehicleController : Controller
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserHelper _userHelper;

        public VehicleController(DataContext context, IMapper mapper, IUserHelper userHelper) 
        {
            _context = context;
            _mapper = mapper;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            var model = _context.Vehicle.AsNoTracking()
                .Select(v => new VehicleViewModel
                {
                    Id=v.Id,
                    Name=v.Name,
                    CreatedDate=v.CreatedDate,
                    ModifiedDate=v.ModifiedDate,
                    IsEnable=v.IsEnable,
                    MotorSerial=v.MotorSerial,
                    Vin=v.Vin,
                    Cylinder=v.Cylinder,
                    Year=v.Year,
                    KmActual=v.KmActual,
                    KmRegistro=v.KmRegistro,
                    ImageUrl=v.ImageUrl,
                    VehicleBrandId= v.VehicleBrand.Id,
                    CountryId=v.Country.Id,
                    FuelId=v.Fuel.Id,
                    ColorId=v.Color.Id,
                    VehicleStatusId=v.VehicleStatus.Id
                });
            return Json(await DataSourceLoader.LoadAsync(model, loadOptions));
        }

        [HttpGet]
        public async Task<IActionResult> GetActivity(DataSourceLoadOptions loadOptions)
        {
            var model = _context.VehicleRecordActivity.AsNoTracking()
                .Select(v => new VehicleRecordActivityViewModel
                {
                    Id = v.Id,
                    VehicleId=v.Vehicle.Id,
                    Km=v.Km,
                    ClientId=v.RegisterBy.Id,
                    CreatedDate = v.CreatedDate,
                    ModifiedDate = v.ModifiedDate
                  
                });
            return Json(await DataSourceLoader.LoadAsync(model, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            try
            {
                var model = new VehicleViewModel();
                var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
                await PopulateModel(model, valuesDict);
                model.CreatedDate = DateTime.UtcNow;
                var user =await _context.Users.FirstAsync(u => u.UserName.Equals(User.Identity.Name));
                model.CreatedBy = user;
                if (!TryValidateModel(model,nameof(model)))
                    return BadRequest(GetFullErrorMessage(ModelState));
                var vehicle = _mapper.Map<VehicleEntity>(model);
                vehicle.KmActual = vehicle.KmRegistro;
                var result = _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return Json(new { result.Entity.Id });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Message", e.Message);
                if (e.InnerException != null) ModelState.AddModelError("InnerException", e.InnerException.ToString());
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }

            }
            return BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> Put(Guid key, string values)
        {
            try
            {
                var model = await _context.Vehicle
                                            .Include(c => c.Color)
                                            .Include(vb => vb.VehicleBrand)
                                            .Include(vb => vb.VehicleStatus)
                                            .Include(vb => vb.Country)
                                            .Include(vb => vb.Fuel)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(item => item.Id == key);
                var modelViewModel = _mapper.Map<VehicleViewModel>(model);

                if (model == null)
                    return StatusCode(409, "Object not found");
                _context.Entry(model).State = EntityState.Detached;
                var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
                await PopulateModel(modelViewModel, valuesDict);
                if (!TryValidateModel(model))
                    return BadRequest(GetFullErrorMessage(ModelState));
                model.ModifiedDate = DateTime.UtcNow;
                model = _mapper.Map<VehicleEntity>(modelViewModel);
                _context.Entry(model).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Message", e.Message);
                if (e.InnerException != null) ModelState.AddModelError("InnerException", e.InnerException.ToString());
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }
                return View("Error");
            }
            
           
        }

        [HttpDelete]
        public async Task Delete(Guid key) {
            var model = await _context.Vehicle.FirstOrDefaultAsync(item => item.Id == key);
          
            _context.Vehicle.Remove(model);
            await _context.SaveChangesAsync();
        }


        #region Details clients

        [HttpGet]
        public async Task<IActionResult> GetDetailsClients(Guid id, DataSourceLoadOptions loadOptions)
        {
            var model = _context.ClientEntityVehicle.Where(e => e.VehicleEntity.Id.Equals(id)).AsNoTracking()
                .Select(v => new ClientEntityVehicleViewModel
                {
                    Id = v.Id,
                    CreatedDate = v.CreatedDate,
                    ModifiedDate = v.ModifiedDate,
                    IsEnable = v.IsEnable,
                    VehicleId=v.VehicleEntity.Id,
                    ClientId=v.ClientEntity.Id
                });

            return Json(await DataSourceLoader.LoadAsync(model, loadOptions));
        }


        [HttpPost]
        public async Task<IActionResult> PostDetailsClients(Guid id, string values)
        {
            try
            {
             
                if (User.Identity != null)
                {
                    var user = await _userHelper.GetUserAsync(User.Identity.Name);
                    var model = new ClientEntityVehicleViewModel();
                    var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
                   // model.VehicleId = id;
                    await PopulateModel(model, valuesDict);
                    model.CreatedDate = DateTime.UtcNow;


                    var clientEntityVehicleEntity = _mapper.Map<ClientEntityVehicleEntity>(model);
                    clientEntityVehicleEntity.CreatedBy = user;
                    clientEntityVehicleEntity.ModifiedBy = null;
                    var result = await _context.ClientEntityVehicle.AddAsync(clientEntityVehicleEntity);
                    await _context.SaveChangesAsync();
                    return Json(new { result.Entity.Id });
                }
            }
            catch (DbUpdateException e)
            {
                if (e.InnerException != null) ModelState.AddModelError("DbUpdateException", e.InnerException.ToString());
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }

            }
           
            catch (Exception e)
            {
                ModelState.AddModelError("Message", e.Message);
                if (e.InnerException != null) ModelState.AddModelError("InnerException", e.InnerException.ToString());
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }

            }
            return BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> PutDetailsClients(Guid key, string values)
        {
            try
            {
                var model = await _context.ClientEntityVehicle
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(item => item.Id == key);
                var modelViewModel = _mapper.Map<ClientEntityVehicleViewModel>(model);

                if (model == null)
                    return StatusCode(409, "Object not found");
                _context.Entry(model).State = EntityState.Detached;
                var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
                await PopulateModel(modelViewModel, valuesDict);
                if (!TryValidateModel(model))
                    return BadRequest(GetFullErrorMessage(ModelState));
                model.ModifiedDate = DateTime.UtcNow;
                model = _mapper.Map<ClientEntityVehicleEntity>(modelViewModel);
                if (User.Identity != null)
                {
                    var user = await _userHelper.GetUserAsync(User.Identity.Name); /*_context.Users.FirstAsync(u => u.UserName.Equals(User.Identity.Name));*/
                    model.ModifiedBy = user;
                }
                _context.Entry(model).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Message", e.Message);
                if (e.InnerException != null) ModelState.AddModelError("InnerException", e.InnerException.ToString());
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }
                return View("Error");
            }


        }

        [HttpDelete]
        public async Task PutDetailsClients(Guid key)
        {
            var model = await _context.ClientEntityVehicle.FirstOrDefaultAsync(item => item.Id == key);
            _context.ClientEntityVehicle.Remove(model);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Lookups

        [HttpGet]
        public async Task<IActionResult> ClientLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Client.AsNoTracking().OrderBy(i => i.Name).Select(i => new { Value = i.Id, Text = i.Name });
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<IActionResult> VehicleLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = _context.Vehicle.OrderBy(i => i.Name).Select(i => new { Value = i.Id, Text = i.Name });
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }
        [HttpGet]
        public async Task<IActionResult> VehicleStatusLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = from i in _context.VehicleStatus
                         orderby i.Name
                         select new
                         {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }
        [HttpGet]
        public async Task<IActionResult> VehicleBrandLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = from i in _context.VehicleBrand
                         orderby i.Name
                         select new
                         {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }
        [HttpGet]
        public async Task<IActionResult> CountryLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = from i in _context.Country
                         orderby i.Name
                         select new
                         {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }
        [HttpGet]
        public async Task<IActionResult> FuelLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = from i in _context.Fuel
                         orderby i.Name
                         select new
                         {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }
        [HttpGet]
        public async Task<IActionResult> ColorLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = from i in _context.Color
                         orderby i.Name
                         select new
                         {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        #endregion


        private async Task PopulateModel(VehicleViewModel model, IDictionary values) {

            try
            {
                const string id = nameof(VehicleViewModel.Id);
                const string name = nameof(VehicleViewModel.Name);
                const string isEnable = nameof(VehicleViewModel.IsEnable);
                const string motorSerial = nameof(VehicleViewModel.MotorSerial);
                const string vin = nameof(VehicleViewModel.Vin);
                const string cylinder = nameof(VehicleViewModel.Cylinder);
                const string year = nameof(VehicleViewModel.Year);
                const string kmRegistro = nameof(VehicleViewModel.KmRegistro);
                const string imageUrl = nameof(VehicleViewModel.ImageUrl);
                const string brandId = nameof(VehicleViewModel.VehicleBrandId);
                const string colorId = nameof(VehicleViewModel.ColorId);
                const string counrtyId = nameof(VehicleViewModel.CountryId);
                const string fuelId = nameof(VehicleViewModel.FuelId);
                const string vehicleStatusId = nameof(VehicleViewModel.VehicleStatusId);

                if (values.Contains(id))
                {
                    model.Id = ConvertTo<Guid>(values[id]);
                }

                if (values.Contains(name))
                {
                    model.Name = Convert.ToString(values[name])?.ToUpper();
                }
                if (values.Contains(isEnable))
                {
                    model.IsEnable = Convert.ToBoolean(values[isEnable]);
                }

                if (values.Contains(motorSerial))
                {
                    model.MotorSerial = Convert.ToString(values[motorSerial])?.ToUpper();
                }

                if (values.Contains(vin))
                {
                    model.Vin = Convert.ToString(values[vin])?.ToUpper();
                }

                if (values.Contains(cylinder))
                {
                    model.Cylinder = Convert.ToInt32(values[cylinder]);
                }

                if (values.Contains(year))
                {
                    model.Year = Convert.ToInt16(values[year]);
                }

                if (values.Contains(kmRegistro))
                {
                    model.KmRegistro = Convert.ToInt64(values[kmRegistro]);
                }

                if (values.Contains(imageUrl))
                {
                    model.ImageUrl = Convert.ToString(values[imageUrl]);
                }

                if (values.Contains(brandId))
                {
                    model.VehicleBrandId = Convert.ToInt16(values[brandId]);
                    model.VehicleBrand = await _context.VehicleBrand.FindAsync(model.VehicleBrandId);
                }

              

                if (values.Contains(colorId))
                {
                    model.ColorId = Convert.ToInt16(values[colorId]);
                    model.Color = await _context.Color.FindAsync(model.ColorId);
                }
                if (values.Contains(counrtyId))
                {
                    model.CountryId = Convert.ToInt16(values[counrtyId]);
                    model.Country = await _context.Country.FindAsync(model.CountryId);
                }
                if (values.Contains(fuelId))
                {
                    model.FuelId = Convert.ToByte(values[fuelId]);
                    model.Fuel = await _context.Fuel.FindAsync(model.FuelId);
                }
                if (values.Contains(vehicleStatusId))
                {
                    model.VehicleStatusId = Convert.ToByte(values[vehicleStatusId]);
                    model.VehicleStatus = await _context.VehicleStatus.FindAsync(model.VehicleStatusId);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
              
            }
            

            //dto.VehicleBrand = await _context.VehicleBrand.FindAsync(model.VehicleBrandId);
           // dto.VehicleType = await _context.VehicleType.FindAsync(model.VehicleTypeId);
            //dto.VehicleStatus = await _context.VehicleStatus.FindAsync(model.VehicleStatusId);
            //dto.Country = await _context.Country.FindAsync(model.CountryId);
            //dto.Fuel = await _context.Fuel.FindAsync(model.FuelId);
            //dto.Color = await _context.Color.FindAsync(model.ColorId);

        }

        private async Task PopulateModel(ClientEntityVehicleViewModel model, IDictionary values)
        {

            try
            {
                const string id = nameof(ClientEntityVehicleViewModel.Id);
                const string isEnable = nameof(ClientEntityVehicleViewModel.IsEnable);
                const string clientId = nameof(ClientEntityVehicleViewModel.ClientId);
                const string vehicleId = nameof(ClientEntityVehicleViewModel.VehicleId);
                const string createdById = nameof(ClientEntityVehicleViewModel.CreatedById);
                const string modifiedById = nameof(ClientEntityVehicleViewModel.ModifiedById);

                if (values.Contains(id))
                {
                    model.Id = ConvertTo<Guid>(values[id]);
                }
               
                if (values.Contains(isEnable))
                {
                    model.IsEnable = Convert.ToBoolean(values[isEnable]);
                }
                if (values.Contains(clientId))
                {
                    model.ClientId = Convert.ToInt16(values[clientId]);
                    model.ClientEntity = await _context.Client.FindAsync(model.ClientId);
                }
                if (values.Contains(vehicleId))
                {
                    model.VehicleId = ConvertTo<Guid>(values[vehicleId]);
                    model.VehicleEntity = await _context.Vehicle.FindAsync(model.VehicleId);
                }

                if (values.Contains(createdById))
                {
                    model.CreatedById = values[createdById].ToString();
                    model.CreatedBy = await _userHelper.GetUserAsync(values[createdById].ToString());
                }

                if (values.Contains(modifiedById))
                {
                    model.ModifiedById = values[modifiedById].ToString();
                    model.ModifiedBy = await _userHelper.GetUserAsync(values[modifiedById].ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }
        private T ConvertTo<T>(object value) {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
        }
        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = (from entry in modelState from error in entry.Value.Errors select error.ErrorMessage).ToList();
            return string.Join(" ", messages);
        }
    }
}