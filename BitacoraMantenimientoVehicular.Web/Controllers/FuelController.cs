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
   
    public class FuelController : Controller
    {
        private readonly DataContext _context;

        public FuelController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) {
            var fuel = _context.Fuel;
            return Json(await DataSourceLoader.LoadAsync(fuel, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new FuelEntity();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));
            model.CreatedDate = DateTime.UtcNow;
            var result = _context.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.Id });
        }

        [HttpPut]
        public async Task<IActionResult> Put(short key, string values) {
            var model = await _context.Fuel.FirstOrDefaultAsync(item => item.Id == key);
            if(model == null)
                return StatusCode(409, "Object not found");

            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));
            model.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task Delete(short key) {
            var model = await _context.Fuel.FirstOrDefaultAsync(item => item.Id == key);

            _context.Fuel.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(FuelEntity model, IDictionary values) {
            const string id = nameof(FuelEntity.Id);
            const string name = nameof(FuelEntity.Name);
            const string isEnable = nameof(FuelEntity.IsEnable);

            if(values.Contains(id)) {
                model.Id = Convert.ToByte(values[id]);
            }

            if(values.Contains(name)) {
                model.Name = Convert.ToString(values[name])?.ToUpper(); ;
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