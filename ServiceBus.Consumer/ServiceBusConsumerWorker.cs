using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Prometheus;

namespace ServiceBus.Consumer
{
    public class ServiceBusConsumerWorker : IHostedService
    {
        private readonly ServiceBusProcessor _processor;
        private static readonly Counter MessagesReceived = Metrics.CreateCounter(
            "servicebus_messages_received",
            "Number of messages received from Service Bus"
        );

        public ServiceBusConsumerWorker(ServiceBusProcessor processor)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Worker started, subscribing to queue...");

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            return _processor.StartProcessingAsync(cancellationToken);
        }
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received message: {body}");

            await args.CompleteMessageAsync(args.Message);
            MessagesReceived.Inc();
            
        }
        private async Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Error: {args.Exception}");
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
    }
}