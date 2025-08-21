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
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("ServiceBus:ConnectionString is not configured.");

        if (string.IsNullOrEmpty(queueName))
            throw new InvalidOperationException("ServiceBus:QueueName is not configured.");

        // Register ServiceBusClient
        builder.Services.AddSingleton(new ServiceBusClient(connectionString));

        // Register ServiceBusProcessor
        builder.Services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false
            });
        });
        // Hosted service to listen to messages
        builder.Services.AddHostedService<ServiceBusConsumerWorker>();

        // Prometheus metrics
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseMetricServer();
        app.UseHttpMetrics();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapGet("/", () => "Service Bus Consumer running...");
        
        app.Run();

    }
}