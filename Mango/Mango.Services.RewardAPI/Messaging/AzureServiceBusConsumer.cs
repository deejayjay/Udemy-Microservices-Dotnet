using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Service.IService;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string _serviceBusConnectionString;
    private readonly string _orderCreatedTopic;
    private readonly string _orderCreatedRewardSubscription;
    private readonly ServiceBusProcessor _rewardProcessor;
    
    private readonly IRewardService _rewardService;
    private readonly IConfiguration _configuration;
    public AzureServiceBusConsumer(IConfiguration configuration, IRewardService rewardService)
    {
        _configuration = configuration;
        _rewardService = rewardService;

        _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")!;
        _orderCreatedTopic = _configuration.GetValue<string>("ServiceBusSettings:OrderCreatedTopicName")!;
        _orderCreatedRewardSubscription = _configuration.GetValue<string>("ServiceBusSettings:OrderCreated_Rewards_Subscription")!;

        var client = new ServiceBusClient(_serviceBusConnectionString);

        _rewardProcessor = client.CreateProcessor(_orderCreatedTopic, _orderCreatedRewardSubscription);
    }

    public async Task StartAsync()
    {
        _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardRequestReceived;
        _rewardProcessor.ProcessErrorAsync += OnNewOrderRewardError;

        await _rewardProcessor.StartProcessingAsync();        
    }

    private async Task OnNewOrderRewardRequestReceived(ProcessMessageEventArgs args)
    {
        // This is where messages are received
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        try
        {
            var messageObj = JsonConvert.DeserializeObject<RewardMessage>(body);

            if (messageObj is not null)
                await _rewardService.UpdateRewardsAsync(messageObj);

            await args.CompleteMessageAsync(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private Task OnNewOrderRewardError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        await _rewardProcessor.StopProcessingAsync();
        await _rewardProcessor.DisposeAsync();
    }
}
