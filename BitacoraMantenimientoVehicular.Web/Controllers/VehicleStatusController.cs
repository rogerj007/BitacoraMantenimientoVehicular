using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BitacoraMantenimientoVehicular.Web.Controllers
{
    [Route("api/[controller]/[action]")]

    public class VehicleStatusController : Controller
    {
        private readonly DataContext _context;

        public VehicleStatusController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            var vehiclestatus = _context.VehicleStatus;

            return Json(await DataSourceLoader.LoadAsync(vehiclestatus, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new VehicleStatusEntity();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));
            model.CreatedDate = DateTime.UtcNow;
            var result = await _context.VehicleStatus.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.Id });
        }

        [HttpPut]
        public async Task<IActionResult> Put(byte key, string values) {
            var model = await _context.VehicleStatus.FirstOrDefaultAsync(item => item.Id == key);
            if(model == null)
                return StatusCode(409, "Object not found");

            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);
            model.ModifiedDate = DateTime.UtcNow;
            if (!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task Delete(byte key) {
            var model = await _context.VehicleStatus.FirstOrDefaultAsync(item => item.Id == key);

            _context.VehicleStatus.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(VehicleStatusEntity model, IDictionary values) {
            const string id = nameof(VehicleStatusEntity.Id);
            const string name = nameof(VehicleStatusEntity.Name);
            const string isEnable = nameof(VehicleStatusEntity.IsEnable);

            if(values.Contains(id)) {
                model.Id = Convert.ToByte(values[id]);
            }
            if(values.Contains(name)) {
                model.Name = Convert.ToString(values[name]);
            }
            if(values.Contains(isEnable)) {
                model.IsEnable = Convert.ToBoolean(values[isEnable]);
            }
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