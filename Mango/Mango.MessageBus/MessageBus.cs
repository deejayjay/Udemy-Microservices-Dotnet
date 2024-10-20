using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace Mango.MessageBus;

public class MessageBus(IConfiguration configuration) : IMessageBus
{
    private readonly IConfiguration _configuration = configuration;

    public async Task PublishMessageAsync(object message, string queueOrTopicName)
    {
        await using var client = new ServiceBusClient(_configuration.GetSection("ServiceBusConnectionString").Value);

        var sender = client.CreateSender(queueOrTopicName);

        var jsonMessage = JsonConvert.SerializeObject(message);

        var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
        {
            CorrelationId = Guid.NewGuid().ToString(),
        }; 

        await sender.SendMessageAsync(serviceBusMessage);        
    }
}
