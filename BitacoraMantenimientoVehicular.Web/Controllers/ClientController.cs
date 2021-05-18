using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using BitacoraMantenimientoVehicular.Datasource.Enums;
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
    public class ClientController : Controller
    {
        private readonly DataContext _context;

        public ClientController(DataContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            var client = _context.Client;
            return Json(await DataSourceLoader.LoadAsync(client, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new ClientEntity();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateModel(model, valuesDict);
            if(!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));
            model.CreatedDate = DateTime.UtcNow;
            var result = await _context.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.Id });
        }

        [HttpPut]
        public async Task<IActionResult> Put(short key, string values) {
            var model = await _context.Client.FirstOrDefaultAsync(item => item.Id == key);
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
        public async Task Delete(short key) {
            var model = await _context.Client.FirstOrDefaultAsync(item => item.Id == key);

            _context.Client.Remove(model);
            await _context.SaveChangesAsync();
        }


        private void PopulateModel(ClientEntity model, IDictionary values) {
            const string colorId = nameof(ClientEntity.Id);
            const string name = nameof(ClientEntity.Name);
            const string isEnable = nameof(ClientEntity.IsEnable);
            const string dni = nameof(ClientEntity.Dni);
            const string mail = nameof(ClientEntity.Mail);
            const string phone = nameof(ClientEntity.Phone);
            const string cellphone = nameof(ClientEntity.CellPhone);
            const string telegram = nameof(ClientEntity.Telegram);
            const string telegramCode = nameof(ClientEntity.TelegramCode);
            const string address = nameof(ClientEntity.Address);
            const string userType = nameof(ClientEntity.UserType);

            if (values.Contains(colorId)) {
                model.Id = Convert.ToInt16(values[colorId]);
            }

            if(values.Contains(name)) {
                model.Name = Convert.ToString(values[name]);
            }
            if(values.Contains(isEnable)) {
                model.IsEnable = Convert.ToBoolean(values[isEnable]);
            }

            if(values.Contains(dni)) {
                model.Dni = Convert.ToString(values[dni]);
            }

            if(values.Contains(mail)) {
                model.Mail = Convert.ToString(values[mail]);
            }

            if(values.Contains(phone)) {
                model.Phone = Convert.ToString(values[phone]);
            }

            if (values.Contains(cellphone))
            {
                model.CellPhone = Convert.ToString(values[cellphone]);
            }

            if (values.Contains(telegram)) {
                model.Telegram = Convert.ToString(values[telegram]);
            }

            if(values.Contains(telegramCode)) {
                model.TelegramCode = Convert.ToString(values[telegramCode]);
            }

            if(values.Contains(address)) {
                model.Address = Convert.ToString(values[address]);
            }

            if (values.Contains(userType))
            {
               model.UserType = (UserType)Enum.Parse(typeof(UserType), values[userType].ToString() ?? string.Empty); 
            }
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = (from entry in modelState from error in entry.Value.Errors select error.ErrorMessage).ToList();
            return string.Join(" ", messages);
        }
    }
}