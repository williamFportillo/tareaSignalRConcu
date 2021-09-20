using Message.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class sendMessagesController : ControllerBase
    {
        [HttpPost]
        public ActionResult<MessageDTO> sendMessage([FromBody] MessageDTO message)
        {
            try
            {
                var json = JsonConvert.SerializeObject(message);
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    Port = 5672
                };

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare("message-queue", false, false, false, null);
                        var body = Encoding.UTF8.GetBytes(json);
                        channel.BasicPublish(string.Empty, "message-queue", null, body);
                    }
                }
                return Ok($"Enviando...");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
