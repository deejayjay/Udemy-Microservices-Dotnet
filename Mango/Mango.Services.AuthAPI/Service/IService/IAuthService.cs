using Mango.Services.AuthAPI.Models.Dto;

namespace Mango.Services.AuthAPI.Service.IService;

public interface IAuthService
{
    Task<string> RegisterAsync(RegistrationRequestDto registrationRequestDto);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
    Task<bool> AssignRoleAsync(string email, string roleName);
}
