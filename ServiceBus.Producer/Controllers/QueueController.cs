using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace ServiceBus.Producer.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly ServiceBusSender _sender;
        private static readonly Counter MessagesSent = Metrics.CreateCounter(
            "servicebus_messages_sent",
            "Number of messages sent to Service Bus"
        );

        public QueueController(ServiceBusSender sender)
        {
            _sender = sender;
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty");

            var serviceBusMessage = new ServiceBusMessage(message);

            await _sender.SendMessageAsync(serviceBusMessage);
            MessagesSent.Inc();

            return Ok($"Message sent: {message}");
        }
    }
    
}