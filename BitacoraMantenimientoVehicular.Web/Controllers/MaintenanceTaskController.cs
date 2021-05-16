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
    public class MaintenanceTaskController : Controller
    {
        private readonly DataContext _context;

        public MaintenanceTaskController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) {
            var maintenancetask = _context.MaintenanceTask;

            return Json(await DataSourceLoader.LoadAsync(maintenancetask, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new MaintenanceTaskEntity();
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
            var model = await _context.MaintenanceTask.FirstOrDefaultAsync(item => item.Id == key);
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
            var model = await _context.MaintenanceTask.FirstOrDefaultAsync(item => item.Id == key);

            _context.MaintenanceTask.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(MaintenanceTaskEntity model, IDictionary values) {
            const string id = nameof(MaintenanceTaskEntity.Id);
            const string name = nameof(MaintenanceTaskEntity.Name);
            const string isEnable = nameof(MaintenanceTaskEntity.IsEnable);
            const string amount = nameof(MaintenanceTaskEntity.Km);

            if (values.Contains(id)) {
                model.Id = Convert.ToInt16(values[id]);
            }
            if(values.Contains(name)) {
                model.Name = Convert.ToString(values[name])?.ToUpper();
            }
            if(values.Contains(isEnable)) {
                model.IsEnable = Convert.ToBoolean(values[isEnable]);
            }
            if (values.Contains(amount))
            {
                model.Km = Convert.ToInt16(values[amount]);
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