using Prometheus;
using Saunter;
using ServiceBus.Prometheus;

namespace ServiceBus.Producer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load connection string safely (from user-secrets or env var)
        var connectionString = builder.Configuration["ServiceBus:ConnectionString"];

        // Register services
        builder.Services.AddSwaggerGen();
        builder.Services.AddHostedService<ServicebusPrometheusExporter>();

        // AsyncAPI support (optional, only if you need it)
        builder.Services.AddAsyncApiSchemaGeneration(options =>
        {
            options.AsyncApi = new Saunter.AsyncApiSchema.v2.AsyncApiDocument
            {
                Info = new Saunter.AsyncApiSchema.v2.Info("Producer API", "1.0.0")
                {
                    Description = "Producer service for Azure Service Bus"
                }
            };
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Metrics
        app.UseMetricServer();   // expose /metrics
        app.UseHttpMetrics();    // capture HTTP request metrics

        // Standard ASP.NET Core pipeline
        app.UseHttpsRedirection();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();
        app.UseAuthorization();

        // Map endpoints
        app.MapControllers();
        app.MapAsyncApiUi();        // AsyncAPI UI (only if schema added)
        app.MapAsyncApiDocuments(); // AsyncAPI JSON

        app.Run();
    }
}
