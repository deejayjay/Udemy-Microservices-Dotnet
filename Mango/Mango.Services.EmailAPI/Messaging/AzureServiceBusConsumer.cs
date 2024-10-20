using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dtos;
using Mango.Services.EmailAPI.Service.IService;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string _serviceBusConnectionString;
    private readonly string _emailCartQueueName;
    private readonly string _registerUserQueueName;
    private readonly ServiceBusProcessor _emailCartProcessor;
    private readonly ServiceBusProcessor _registerUserProcessor;

    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    public AzureServiceBusConsumer(IConfiguration configuration, IEmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;

        _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")!;
        _emailCartQueueName = _configuration.GetValue<string>("ServiceBusSettings:ShoppingCartQueueName")!;
        _registerUserQueueName = _configuration.GetValue<string>("ServiceBusSettings:RegisterUserQueueName")!;

        var client = new ServiceBusClient(_serviceBusConnectionString);
        
        _emailCartProcessor = client.CreateProcessor(_emailCartQueueName);
        _registerUserProcessor = client.CreateProcessor(_registerUserQueueName);
    }

    public async Task StartAsync()
    {
        _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
        _emailCartProcessor.ProcessErrorAsync += OnEmailCartError;        
        
        await _emailCartProcessor.StartProcessingAsync();

        _registerUserProcessor.ProcessMessageAsync += OnEmailUserRegisteredReceived;
        _registerUserProcessor.ProcessErrorAsync += OnEmailUserRegisteredError;

        await _registerUserProcessor.StartProcessingAsync();
    }

    private async Task OnEmailUserRegisteredReceived(ProcessMessageEventArgs args)
    {
        // This is where messages are received
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        try
        {
            var messageObj = JsonConvert.DeserializeObject<string>(body);

            if (messageObj is not null)
                await _emailService.EmailAndLogUserRegistrationAsync(messageObj);

            await args.CompleteMessageAsync(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private Task OnEmailUserRegisteredError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
    {
        // This is where messages are received
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        try
        {
            var messageObj = JsonConvert.DeserializeObject<CartDto>(body);
            
            if (messageObj is not null)
                await _emailService.EmailAndLogCartAsync(messageObj);

            await args.CompleteMessageAsync(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private Task OnEmailCartError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        await _emailCartProcessor.StopProcessingAsync();
        await _emailCartProcessor.DisposeAsync();

        await _registerUserProcessor.StopProcessingAsync();
        await _registerUserProcessor.DisposeAsync();
    }
}
