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

        // Create a listener for OpenTelemetry metrics
        _listener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "ServiceBus" &&
                    _instrumentations.Contains(instrument.Name))
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            },
            MeasurementsCompleted = (instrument, state) => { }
        };

        // When a measurement is recorded, map it to Prometheus
        _listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            var (labels, values) = ExtractLabels(tags);
            if (_counters.TryGetValue(instrument.Name, out var counter))
            {
                counter.WithLabels(values).Inc(measurement);
            }
            else if (_histograms.TryGetValue(instrument.Name, out var histogram))
            {
                histogram.WithLabels(values).Observe(measurement);
            }
        });
        _listener.SetMeasurementEventCallback<int>((instrument, measurement, tags, state) =>
        {
            if (_counters.TryGetValue(instrument.Name, out var counter))
            {
                var (labelKeys, labelValues) = ExtractLabels(tags);
                counter.WithLabels(labelValues).Inc(measurement);
            }
        });

        _listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            if (_histograms.TryGetValue(instrument.Name, out var histogram))
            {
                var (labelKeys, labelValues) = ExtractLabels(tags);
                histogram.WithLabels(labelValues).Observe(measurement);
            }
        });

        _listener.Start();

        
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
