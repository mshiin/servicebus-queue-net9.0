using Azure.Messaging.ServiceBus;
using Prometheus;
using ServiceBus.Prometheus;

namespace ServiceBus.Producer;

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
            return client.CreateSender(queueName);
        });

        builder.Services.AddHostedService<ServicebusPrometheusExporter>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseMetricServer();
        app.UseHttpMetrics();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapPost("/send", async (ServiceBusSender sender, string message) =>
        {
            await sender.SendMessageAsync(new ServiceBusMessage(message));
            return Results.Ok($"Sent: {message}");
        });

        app.Run();
    }
}
