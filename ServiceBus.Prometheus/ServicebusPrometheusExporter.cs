using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using Prometheus;


namespace ServiceBus.Prometheus;

public class ServicebusPrometheusExporter : IHostedService, IDisposable
{
    private readonly Dictionary<string, Counter> _counters = new();
    private readonly Dictionary<string, Histogram> _histograms = new();
    private static readonly HashSet<string> _instrumentations =
    [
        ServiceBusMeter.ServiceBusMessageSent,
        ServiceBusMeter.ServiceBusMessageReceived,
        ServiceBusMeter.ServiceBusMessageDeliveryCount,
        ServiceBusMeter.ServiceBusMessageQueueLatency
    ];
    private MeterListener _listener;
    public void Dispose()
    {

    }
    private static readonly string[] LabelNames = ["clientType", "resourceId", "payloadTypeId"];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CreateCounter(ServiceBusMeter.ServiceBusMessageSent, LabelNames);
        CreateCounter(ServiceBusMeter.ServiceBusMessageReceived, LabelNames);
        CreateHistogram(ServiceBusMeter.ServiceBusMessageDeliveryCount, LabelNames);
        CreateHistogram(ServiceBusMeter.ServiceBusMessageQueueLatency, LabelNames);
        
        // TODO: hook up EventListener here
        return Task.CompletedTask;

    }
    private static (string[] labels, string?[] labelValues) ExtractLabels(
        ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var values = new string[LabelNames.Length];
        for (var i = 0; i < LabelNames.Length; i++)
        {
            values[i] = "";
            foreach (var tag in tags)
            {
                if (tag.Key != LabelNames[i] || tag.Value is null)
                    continue;
                values[i] = tag.Value?.ToString();
                break;
            }
        }
        return (LabelNames, values);
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
