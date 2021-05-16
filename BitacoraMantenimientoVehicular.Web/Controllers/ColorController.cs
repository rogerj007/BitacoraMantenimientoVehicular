using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class ColorController : Controller
    {
        private readonly DataContext _context;

        public ColorController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) {
            var color =_context.Color.OrderBy(c=>c.Name);
            return Json(await DataSourceLoader.LoadAsync(color, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new ColorEntity();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);

            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));
            model.CreatedDate = DateTime.UtcNow;
            var result = await _context.Color.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.Id });
        }

        [HttpPut]
        public async Task<IActionResult> Put(short key, string values) {
            var model = await _context.Color.FirstOrDefaultAsync(item => item.Id == key);
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
            var model = await _context.Color.FirstOrDefaultAsync(item => item.Id == key);

            _context.Color.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(ColorEntity model, IDictionary values) {
            string ID = nameof(ColorEntity.Id);
            string NAME = nameof(ColorEntity.Name);
            string IS_ENABLE = nameof(ColorEntity.IsEnable);
            if(values.Contains(ID)) {
                model.Id = Convert.ToInt16(values[ID]);
            }
            if(values.Contains(NAME)) {
                model.Name = Convert.ToString(values[NAME]).ToUpper();
            }
            if (values.Contains(IS_ENABLE)) {
                model.IsEnable = Convert.ToBoolean(values[IS_ENABLE]);
            }
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = new List<string>();

            foreach(var entry in modelState) {
                foreach(var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return string.Join(" ", messages);
        }
    }
}