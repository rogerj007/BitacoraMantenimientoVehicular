using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
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

        public VehicleController(DataContext context, IMapper mapper) {
            _context = context;
            _mapper = mapper;
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
                    KmHrActual=v.KmHrActual,
                    ImageUrl=v.ImageUrl,
                    VehicleBrandId= v.VehicleBrand.Id,
                    CountryId=v.Country.Id,
                    FuelId=v.Fuel.Id,
                    ColorId=v.Color.Id,
                    VehicleStatusId=v.VehicleStatus.Id
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
        public async Task<IActionResult> VehicleBrandLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.VehicleBrand
                         orderby i.Name
                         select new {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

       

        [HttpGet]
        public async Task<IActionResult> CountryLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Country
                         orderby i.Name
                         select new {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<IActionResult> FuelLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Fuel
                         orderby i.Name
                         select new {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        [HttpGet]
        public async Task<IActionResult> ColorLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Color
                         orderby i.Name
                         select new {
                             Value = i.Id,
                             Text = i.Name
                         };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

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
                const string kmHrActual = nameof(VehicleViewModel.KmHrActual);
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

                if (values.Contains(kmHrActual))
                {
                    model.KmHrActual = Convert.ToInt64(values[kmHrActual]);
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

        private T ConvertTo<T>(object value) {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = new List<string>();

            foreach(var entry in modelState) {
                foreach(var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return String.Join(" ", messages);
        }
    }
}