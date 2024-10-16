﻿using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers;

public class AuthController(IAuthService authService, ITokenProvider tokenProvider) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    [HttpGet]
    public IActionResult Login()
    {
        LoginRequestDto loginRequestDto = new();

        return View(loginRequestDto);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
    {
        var responseDto = await _authService.LoginAsync(model);

        if (responseDto is not null && responseDto.IsSuccess)
        {
            var loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result)!);

            await SignInUserAsync(loginResponseDto!);

            _tokenProvider.SetToken(loginResponseDto!.Token);

            // Redirect to the ReturnUrl if it's valid
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        TempData["error"] = responseDto?.Message;

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        PopulateRoleList();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegistrationRequestDto model)
    {
        var result = await _authService.RegisterAsync(model);

        ResponseDto? assignRole;

        if (result is not null && result.IsSuccess)
        {
            if (string.IsNullOrWhiteSpace(model.Role))
            {
                model.Role = SD.RoleCustomer;
            }

            assignRole = await _authService.AssignRoleAsync(model);

            if (assignRole is not null && assignRole.IsSuccess)
            {
                TempData["success"] = "Registration Successful";
                return RedirectToAction(nameof(Login));
            }
        }
        else
        {
            TempData["error"] = result?.Message;
        }

        PopulateRoleList();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        _tokenProvider.ClearToken();

        return RedirectToAction("Index", "Home");
    }

    private void PopulateRoleList()
    {
        var roleList = new List<SelectListItem>()
        {
            new() { Text = SD.RoleAdmin, Value = SD.RoleAdmin },
            new() { Text = SD.RoleCustomer, Value = SD.RoleCustomer }
        };

        ViewBag.RoleList = roleList;
    }

    private async Task SignInUserAsync(LoginResponseDto model)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(model.Token);

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email, jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email).Value));
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub).Value));
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name, jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name).Value));
        
        identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email).Value));
        identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(c => c.Type == "role").Value));

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
