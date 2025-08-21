using Azure.Messaging.ServiceBus;
using Prometheus;

namespace ServiceBus.Consumer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration["ServiceBus:ConnectionString"];
        var queueName = builder.Configuration["ServiceBus:QueueName"];

        builder.Services.AddSingleton(new ServiceBusClient(connectionString));
        builder.Services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false
            });
        });

        builder.Services.AddHostedService<ServiceBusConsumerWorker>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseMetricServer();
        app.UseHttpMetrics();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapGet("/", () => "Consumer running...");
        app.Run();
    }
}
