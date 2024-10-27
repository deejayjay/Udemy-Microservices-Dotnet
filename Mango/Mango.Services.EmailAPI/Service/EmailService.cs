using Microsoft.EntityFrameworkCore;
using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models.Dtos;
using Mango.Services.EmailAPI.Service.IService;
using System.Text;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Message;

namespace Mango.Services.EmailAPI.Service;

public class EmailService : IEmailService
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public EmailService(DbContextOptions<AppDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
    }

    public async Task EmailAndLogCartAsync(CartDto cartDto)
    {
        var sb = new StringBuilder();

        sb.AppendLine(@$"<br /> Cart Email Requested
                         <br /> Total: {cartDto.CartHeader.CartTotal:c} 
                         <br />");

        if (cartDto.CartDetails is not null)
        {
            sb.AppendLine("<ul>");

            foreach (var item in cartDto.CartDetails)
            {
                sb.AppendLine($"<li>{item.Product?.Name} x {item.Count}</li>");
            }

            sb.Append("</ul>");
        }

        await LogAndEmailAsync(sb.ToString(), cartDto.CartHeader.Email!);
    }

    public async Task EmailAndLogUserRegistrationAsync(string email)
    {
        var message = $"<p>User registered successfully. <br />Email: {email}</p>";

        await LogAndEmailAsync(message, "deejayjay@dotnetmastery.com");
    }

    public async Task LogOrderPlacedAsync(RewardMessage rewardMessage)
    {
        var message = @$"<div>
                            <p>New order placed.</p>
                            <p><strong>Order ID: </strong>{rewardMessage.OrderId}
                        </div>";

        await LogAndEmailAsync(message, "deejayjay@dotnetmastery.com");
    }

    private async Task<bool> LogAndEmailAsync(string message, string email)
    {
        try
        {
            var emailLog = new EmailLogger
            {
                Email = email,
                Message = message,
                EmailSent = DateTime.Now
            };

            await using var _db = new AppDbContext(_dbOptions);

            await _db.AddAsync(emailLog);
            await _db.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }
    }
}
