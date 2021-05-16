using System;
using System.Collections;
using System.Collections.Generic;
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
            var result = _context.Component.Add(model);
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
            string COLOR_ID = nameof(ComponentEntity.Id);
            string NAME = nameof(ComponentEntity.Name);
            string CODE = nameof(ComponentEntity.Code);
            string TTL = nameof(ComponentEntity.Ttl);
            
            string IS_ENABLE = nameof(ComponentEntity.IsEnable);

            if(values.Contains(COLOR_ID)) {
                model.Id = Convert.ToInt16(values[COLOR_ID]);
            }

            if(values.Contains(NAME)) {
                model.Name = Convert.ToString(values[NAME]).ToUpper();
            }

            if(values.Contains(CODE)) {
                model.Code = Convert.ToString(values[CODE]).ToUpper();
            }

            if(values.Contains(TTL)) {
                model.Ttl = Convert.ToInt16(values[TTL]);
            }

            if(values.Contains(IS_ENABLE)) {
                model.IsEnable = Convert.ToBoolean(values[IS_ENABLE]);
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