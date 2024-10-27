using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Message;
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
    private readonly string _orderCreatedTopicName;
    private readonly string _orderCreatedEmailSubscription;
    private readonly ServiceBusProcessor _emailCartProcessor;
    private readonly ServiceBusProcessor _registerUserProcessor;
    private readonly ServiceBusProcessor _emailOrderPlacedProcessor;

    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    public AzureServiceBusConsumer(IConfiguration configuration, IEmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;

        _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")!;
        _emailCartQueueName = _configuration.GetValue<string>("ServiceBusSettings:ShoppingCartQueueName")!;
        _registerUserQueueName = _configuration.GetValue<string>("ServiceBusSettings:RegisterUserQueueName")!;
        _orderCreatedTopicName = _configuration.GetValue<string>("ServiceBusSettings:OrderCreatedTopicName")!;
        _orderCreatedEmailSubscription = _configuration.GetValue<string>("ServiceBusSettings:OrderCreated_Email_Subscription")!;

        var client = new ServiceBusClient(_serviceBusConnectionString);
        
        _emailCartProcessor = client.CreateProcessor(_emailCartQueueName);
        _registerUserProcessor = client.CreateProcessor(_registerUserQueueName);
        _emailOrderPlacedProcessor = client.CreateProcessor(_orderCreatedTopicName, _orderCreatedEmailSubscription);
    }

    public async Task StartAsync()
    {
        _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
        _emailCartProcessor.ProcessErrorAsync += OnEmailCartError;        
        
        await _emailCartProcessor.StartProcessingAsync();

        _registerUserProcessor.ProcessMessageAsync += OnEmailUserRegisteredReceived;
        _registerUserProcessor.ProcessErrorAsync += OnEmailUserRegisteredError;

        await _registerUserProcessor.StartProcessingAsync();

        _emailOrderPlacedProcessor.ProcessMessageAsync += OnEmailOrderPlaceddReceived;
        _emailOrderPlacedProcessor.ProcessErrorAsync += OnEmailOrderPlacedError;

        await _emailOrderPlacedProcessor.StartProcessingAsync();
    }

    private async Task OnEmailOrderPlaceddReceived(ProcessMessageEventArgs args)
    {
        // This is where messages are received
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        try
        {
            var messageObj = JsonConvert.DeserializeObject<RewardMessage>(body);

            if (messageObj is not null)
                await _emailService.LogOrderPlacedAsync(messageObj);

            await args.CompleteMessageAsync(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private Task OnEmailOrderPlacedError(ProcessErrorEventArgs args)
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

        await _emailOrderPlacedProcessor.StopProcessingAsync();
        await _emailOrderPlacedProcessor.DisposeAsync();
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
}
