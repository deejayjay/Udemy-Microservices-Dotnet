using Mango.MessageBus;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthAPIController(IAuthService authService, IMessageBus messageBus, IConfiguration configuration) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IMessageBus _messageBus = messageBus;
    private readonly IConfiguration _configuration = configuration;

    private ResponseDto _response = new();

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegistrationRequestDto request)
    {
        var message = await _authService.RegisterAsync(request);

        if(!string.IsNullOrWhiteSpace(message))
        {
            _response.IsSuccess = false;
            _response.Message = message;

            return BadRequest(_response);
        }

        // Registration successful - Send email
        await _messageBus.PublishMessageAsync(request.Email, _configuration.GetValue<string>("ServiceBusSettings:RegisterUserQueueName")!);

        return Ok(_response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
    {
        var loginResponse = await _authService.LoginAsync(request);

        if (loginResponse.User is null)
        {
            _response.IsSuccess = false;
            _response.Message = "Username or password is incorrect";

            return BadRequest(_response);
        }

        _response.Result = loginResponse;
        
        return Ok(_response);
    }

    [HttpPost("assignrole")]
    public async Task<IActionResult> AssignRoleAsync([FromBody] RegistrationRequestDto request)
    {
        var assignSuccessful = await _authService.AssignRoleAsync(request.Email, request.Role!.ToUpper());

        if (!assignSuccessful)
        {
            _response.IsSuccess = false;
            _response.Message = "Error encountered while assigning role";

            return BadRequest(_response);
        }

        return Ok(_response);
    }
}
