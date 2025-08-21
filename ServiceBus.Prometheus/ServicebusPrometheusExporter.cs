using System.Reflection.Emit;
using Microsoft.Extensions.Hosting;
using Prometheus;


namespace ServiceBus.Prometheus;

public class ServicebusPrometheusExporter : IHostedService, IDisposable
{
    private readonly Dictionary<string, Counter> _counters = new();
    private readonly Dictionary<string, Histogram> _histograms = new();

    public void Dispose()
    {
        throw new NotImplementedException();
    }
    private static readonly string[] LabelNames = ["clientType", "resourceId", "payloadTypeId"];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CreateCounter(ServiceBusMeter.ServiceBusMessageSent, LabelNames);
        CreateCounter(ServiceBusMeter.ServiceBusMessageReceived, LabelNames);
        CreateHistogram(ServiceBusMeter.ServiceBusMessageDeliveryCount, LabelNames);
        CreateHistogram(ServiceBusMeter.ServiceBusMessageQueueLatency, LabelNames);

    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    private void CreateCounter(string name, string[] labels) {
        var counter = Metrics.CreateCounter(name.Replace('.', '_'), $"Counter for {name}", labels);
        _counters[name] = counter;
    }
    private void CreateHistogram(string name, string[] labels)
    {
        var histogram = Metrics.CreateHistogram(name.Replace('.', '_'), $"Histogram for {name}", labels);
        _histograms[name] = histogram;
    }

}
