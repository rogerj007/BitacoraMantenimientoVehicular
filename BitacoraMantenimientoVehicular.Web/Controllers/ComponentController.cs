using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BitacoraMantenimientoVehicular.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ComponentController : Controller
    {
        private readonly DataContext _context;

        public ComponentController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) {
            var component = _context.Component;
            return Json(await DataSourceLoader.LoadAsync(component, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new ComponentEntity();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));
            model.CreatedDate = DateTime.UtcNow;
            var result = await _context.Component.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.Id });
        }

        [HttpPut]
        public async Task<IActionResult> Put(short key, string values) {
            var model = await _context.Component.FirstOrDefaultAsync(item => item.Id == key);
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
            var model = await _context.Component.FirstOrDefaultAsync(item => item.Id == key);

            _context.Component.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(ComponentEntity model, IDictionary values) {
            const string colorId = nameof(ComponentEntity.Id);
            const string name = nameof(ComponentEntity.Name);
            const string code = nameof(ComponentEntity.Code);
            const string ttl = nameof(ComponentEntity.Ttl);
            const string isEnable = nameof(ComponentEntity.IsEnable);

            if(values.Contains(colorId)) {
                model.Id = Convert.ToInt16(values[colorId]);
            }

            if(values.Contains(name)) {
                model.Name = Convert.ToString(values[name])?.ToUpper();
            }

            if(values.Contains(code)) {
                model.Code = Convert.ToString(values[code])?.ToUpper();
            }

            if(values.Contains(ttl)) {
                model.Ttl = Convert.ToInt64(values[ttl]);
            }

            if(values.Contains(isEnable)) {
                model.IsEnable = Convert.ToBoolean(values[isEnable]);
            }
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = (from entry in modelState from error in entry.Value.Errors select error.ErrorMessage).ToList();

            return string.Join(" ", messages);
        }
    }
}