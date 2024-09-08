using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            throw new NotImplementedException();
        }

        public async Task<string> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            var newUser = new ApplicationUser
            { 
                UserName = registrationRequestDto.Email,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                Name = registrationRequestDto.Name,
                PhoneNumber = registrationRequestDto.PhoneNumber
            };

            try
            {
                var result = await _userManager.CreateAsync(newUser, registrationRequestDto.Password);

                if (result.Succeeded)
                {
                    var userFromDb = await _db.Users.FirstAsync(u => u.UserName == registrationRequestDto.Email);

                    if (userFromDb is not null)
                    {
                        return string.Empty;
                    }                    
                }
                else
                {
                    return result.Errors.FirstOrDefault()?.Description ?? "Error encountered while registering the user";
                }
            }
            catch (Exception)
            {
                throw;
            }

            return "Error encountered while registering the user";
        }
    }
}
