using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Net;
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
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserHelper _userHelper;

        public AccountController(DataContext context, IUserHelper userHelper, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var model = _context.Users.Include(r => r.UserFunction).AsNoTracking()
                    .Select(v => new UserViewModel
                    {
                        Id = v.Id,
                        FirstName = v.FirstName,
                        LastName = v.LastName,
                        UserName = v.UserName,
                        UserFunctionId = v.UserFunction.Id,
                        PhoneNumber = v.PhoneNumber,
                        Document = v.Document,
                        Telegram = v.Telegram,
                        CreatedDate = v.CreatedDate,
                        ModifiedDate = v.ModifiedDate,
                        IsEnable = v.IsEnable,

                    });
                return Json(await DataSourceLoader.LoadAsync(model, loadOptions));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values)
        {
            var model = new UserViewModel();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            await PopulateModel(model, valuesDict);
            model.CreatedDate = DateTime.UtcNow;
            if (!TryValidateModel(model))
                return BadRequest(GetFullErrorMessage(ModelState));

            var user = _mapper.Map<UserEntity>(model);
            var result = await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return Json(new { result.Entity.Id });
        }

        [HttpPut]
        public async Task<IActionResult> Put(string key, string values)
        {
            try
            {
                var model = await _context.Users
                    .Include(c => c.UserFunction)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.Id == key);
                var modelViewModel = _mapper.Map<UserViewModel>(model);

                if (model == null)
                    return StatusCode(409, "Object not found");
                _context.Entry(model).State = EntityState.Detached;
                var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
                await PopulateModel(modelViewModel, valuesDict);
                if (!TryValidateModel(model))
                    return BadRequest(GetFullErrorMessage(ModelState));
                model = _mapper.Map<UserEntity>(modelViewModel);
                model.ModifiedDate = DateTime.UtcNow;
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

        [HttpGet]
        public async Task<IActionResult> UserFunctionLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = from i in _context.UserFunction
                orderby i.Name
                select new
                {
                    Value = i.Id,
                    Text = i.Name
                };
            return Json(await DataSourceLoader.LoadAsync(lookup, loadOptions));
        }

        private async Task PopulateModel(UserViewModel model, IDictionary values)
        {
            const string id = nameof(UserViewModel.Id);
            const string document = nameof(UserViewModel.Document);
            const string phoneNumber = nameof(UserViewModel.PhoneNumber);
            const string firstName = nameof(UserViewModel.FirstName);
            const string lastName = nameof(UserViewModel.LastName);
            const string telegram = nameof(UserViewModel.Telegram);
            const string address = nameof(UserViewModel.Address);
            const string isEnable = nameof(UserViewModel.IsEnable);
            const string userName = nameof(UserViewModel.UserName);
            const string userFuncionId = nameof(UserViewModel.UserFunctionId);


            if (values.Contains(id))
            {
                model.Id = Convert.ToString(values[id]);
            }

            if (values.Contains(userName))
            {
                model.UserName = Convert.ToString(values[userName]);
            }
            if (values.Contains(phoneNumber))
            {
                model.PhoneNumber = Convert.ToString(values[phoneNumber]);
            }
            if (values.Contains(document))
            {
                model.Document = Convert.ToString(values[document]) ?? string.Empty;
            }
            if (values.Contains(isEnable))
            {
                model.IsEnable = Convert.ToBoolean(values[isEnable]);
            }

            if (values.Contains(firstName))
            {
                model.FirstName = Convert.ToString(values[firstName])?.ToUpper() ?? string.Empty;
            }

            if (values.Contains(lastName))
            {
                model.LastName = Convert.ToString(values[lastName])?.ToUpper() ?? string.Empty;
            }
            if (values.Contains(telegram))
            {
                model.Telegram = Convert.ToString(values[telegram]);
            }
            if (values.Contains(address))
            {
                model.Address = Convert.ToString(values[address])?.ToUpper() ?? string.Empty;
            }
            if (values.Contains(userFuncionId))
            {
                model.UserFunctionId = Convert.ToByte(values[userFuncionId]);
                model.UserFunction = await _context.UserFunction.FindAsync(model.UserFunctionId);
            }


        }

        private T ConvertTo<T>(object value)
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFrom(null!, CultureInfo.InvariantCulture, value);
        }
        private string GetFullErrorMessage(ModelStateDictionary modelState)
        {
            var messages = (from entry in modelState from error in entry.Value.Errors select error.ErrorMessage).ToList();

            return string.Join(" ", messages);
        }

        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }
            return RedirectToPage("/Index");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(Request.Query["ReturnUrl"].First());
                    }
                    return RedirectToPage("/Index");
                    //return RedirectToAction("Index", "Home");
                }
                if(result.IsNotAllowed)
                    return Unauthorized();
                //if (result.IsLockedOut)
                //{
                //   // _logger.LogWarning(2, "User account locked out.");
                //    return View("Lockout");
                //}

                //ModelState.AddModelError(string.Empty, "נסיון ההתחברות נכשל");
                //return View(model);
            }
            ModelState.AddModelError(string.Empty, "Failed to login.");
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Ok(model);
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return null;
            if (User.Identity == null) return null;
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user != null)
            {
                var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToPage("/Index");
                }

                ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault()?.Description ?? string.Empty);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User no found.");
            }

            return null;
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToPage("/Index");
        }

    }
}
