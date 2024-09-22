using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService(AppDbContext db, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        IJwtTokenGenerator jwtTokenGenerator) : IAuthService
    {
        private readonly AppDbContext _db = db;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

        public async Task<bool> AssignRoleAsync(string email, string roleName)
        {
            var userFromDb = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email!.ToLower() == email.ToLower());

            if (userFromDb is null) 
                return false;
            
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                // Create role if it does not exist
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            await _userManager.AddToRoleAsync(userFromDb, roleName);
            return true;            
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var userFromDb = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName!.ToLower() == loginRequestDto.UserName.ToLower());

            if (userFromDb is null || !await _userManager.CheckPasswordAsync(userFromDb, loginRequestDto.Password))
            {
                return new LoginResponseDto
                {
                    User = null,
                    Token = string.Empty
                };
            }

            // User was found. Generate JWT.
            var roles = await _userManager.GetRolesAsync(userFromDb);

            UserDto userDto = new() 
            {
                Id = userFromDb.Id,
                Email = userFromDb.Email!,
                Name = userFromDb.Name,
                PhoneNumber = userFromDb.PhoneNumber!
            };

            LoginResponseDto loginResponseDto = new()
            {
                User = userDto, 
                Token = _jwtTokenGenerator.GenerateToken(userFromDb, roles)
            };

            return loginResponseDto;
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
