using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SignalRChat.Hubs;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRChat
{
    public class MessageBackgroundService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly EventingBasicConsumer _consumer;

        private readonly ChatHub hubContext;
        public MessageBackgroundService(ChatHub hubContext)
        {
            this.hubContext = hubContext;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare("message-queue", false, false, false, null);
            _consumer = new EventingBasicConsumer(_channel);
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _consumer.Received += async (model, content) =>
            {
                var body = content.Body.ToArray();
                var result = Encoding.UTF8.GetString(body);
                await hubContext.Clients.All.SendAsync(result);

            };

            _channel.BasicConsume("message-queue", true, _consumer);
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
