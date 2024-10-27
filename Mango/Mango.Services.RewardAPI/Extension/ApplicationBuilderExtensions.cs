using Mango.Services.RewardAPI.Messaging;

namespace Mango.Services.RewardAPI.Extension;

public static class ApplicationBuilderExtensions
{
    private static IAzureServiceBusConsumer? ServiceBusConsumer { get; set; }

    public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
    {
        ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>()
                            ?? throw new InvalidOperationException("Please make sure IAzureServiceBusConsumer is registered in the DI container!");

        var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();
        hostApplicationLife.ApplicationStarted.Register(OnStart);
        hostApplicationLife.ApplicationStopping.Register(OnStop);

        return app;
    }

    private static void OnStop()
    {
        ServiceBusConsumer.StopAsync();
    }

    private static void OnStart()
    {
        ServiceBusConsumer.StartAsync();
    }
}
