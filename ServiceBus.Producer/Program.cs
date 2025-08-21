using Prometheus;
using Saunter;
using ServiceBus.Prometheus;

namespace ServiceBus.Producer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        if (builder.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseMetricServer();

        app.UseMetricServer();
        app.UseHttpMetrics();
        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();
        app.MapAsyncApiUi();
        app.MapAsyncApiDocuments();
        app.Run();

    }
    private static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSwaggerGen();
        builder.Services.AddHostedService<ServicebusPrometheusExporter>();
        return builder;
    }
}

