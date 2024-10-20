namespace Mango.MessageBus;

public interface IMessageBus
{
    Task PublishMessageAsync(object message, string queueOrTopicName);
}
