using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        private ResponseDto _response = new();

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto request)
        {
            var message = await _authService.RegisterAsync(request);

            if(!string.IsNullOrWhiteSpace(message))
            {
                _response.IsSuccess = false;
                _response.Message = message;

                return BadRequest(_response);
            }

            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            return Ok();
        }
    }
}
