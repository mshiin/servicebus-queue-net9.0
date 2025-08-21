using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;

namespace ServiceBus.Producer.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly ServiceBusSender _sender;
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

            return Ok($"Message sent: {message}");
        }
    }
    
}