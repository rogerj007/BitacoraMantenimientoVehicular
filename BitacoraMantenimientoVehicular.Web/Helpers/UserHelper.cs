using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitacoraMantenimientoVehicular.Datasource;
using BitacoraMantenimientoVehicular.Datasource.Entities;
using BitacoraMantenimientoVehicular.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BitacoraMantenimientoVehicular.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly DataContext _context;

        public UserHelper(
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<UserEntity> signInManager,
            DataContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(UserEntity user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(UserEntity user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<UserEntity> GetUserAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> ChangePasswordAsync(UserEntity user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task<IdentityResult> UpdateUserAsync(UserEntity user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<List<UserEntity>> GetAllUserAsync()
        {
            return await _context.Users.Where(u => u.IsEnable)
                .ToListAsync();

        }


        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            try
            {
                var user = _userManager.FindByEmailAsync(model.Username).Result;
                if (user == null || !user.IsEnable) return new SignInResult();
                var result = await _signInManager.PasswordSignInAsync(
                    model.Username,
                    model.Password,
                    model.RememberMe,
                    false);

                return result;
            }
            catch (Exception exception)
            {
               
                return new SignInResult();
            }
           

        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        public async Task<IdentityResult> AddUserAsync(UserEntity user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddUserToRoleAsync(UserEntity user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        public async Task<UserEntity> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> IsUserInRoleAsync(UserEntity user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        //public async Task<UserEntity> AddUserAsync(AddUserViewModel model, string path)
        //{
        //    var userEntity = new UserEntity
        //    {
        //        Address = model.Address,
        //        Document = model.Document,
        //        Email = model.Username,
        //        FirstName = model.FirstName,
        //        LastName = model.LastName,
        //        PicturePath = path,
        //        PhoneNumber = model.PhoneNumber,
        //        UserName = model.Username,
        //        UserType = model.UserTypeId == 1 ? UserType.Supervisor : UserType.User,
        //        UserFunction = await _context.UserFunction.FindAsync(model.UserFuncionId),
        //        Enable = false
        //    };

        //    IdentityResult result = await _userManager.CreateAsync(userEntity, model.Password);
        //    if (result != IdentityResult.Success)
        //    {
        //        return null;
        //    }

        //    UserEntity newUser = await GetUserByEmailAsync(model.Username);
        //    await AddUserToRoleAsync(newUser, userEntity.UserType.ToString());
        //    return newUser;
        //}

        public async Task<UserEntity> GetUserAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.UserFunction)
                //.Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == userId.ToString());
        }
    }

}
