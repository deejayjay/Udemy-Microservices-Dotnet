﻿using Mango.Services.EmailAPI.Models.Dtos;

namespace Mango.Services.EmailAPI.Service.IService;

public interface IEmailService
{
    Task EmailAndLogCartAsync(CartDto cartDto);
    Task EmailAndLogUserRegistrationAsync(string email);
}