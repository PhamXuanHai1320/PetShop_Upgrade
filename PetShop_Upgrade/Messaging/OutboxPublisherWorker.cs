using Microsoft.Extensions.Options;
using PetShop_Upgrade.Repositories.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace PetShop_Upgrade.Messaging
{
    public class OutboxPublisherWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<OutboxPublisherWorker> _logger;

        public OutboxPublisherWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<OutboxPublisherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishPendingMessagesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không thể publish OutboxMessage tới RabbitMQ");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var now = DateTime.UtcNow;
            var messages = (await unitOfWork.OutboxMessageRepository.FindAsync(x =>
                    x.PublishedAt == null && (x.NextRetryAt == null || x.NextRetryAt <= now)))
                .OrderBy(x => x.CreatedAt)
                .Take(50)
                .ToList();

            if (messages.Count == 0)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                ClientProvidedName = "petshop-outbox-publisher"
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(true, true), cancellationToken);
            await channel.ExchangeDeclareAsync(
                _options.ExchangeName, ExchangeType.Topic, durable: true,
                autoDelete: false, cancellationToken: cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        ContentType = "application/json",
                        MessageId = message.Id.ToString(),
                        Type = message.EventType
                    };
                    await channel.BasicPublishAsync(
                        _options.ExchangeName,
                        message.EventType,
                        mandatory: true,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes(message.Payload),
                        cancellationToken: cancellationToken);

                    message.PublishedAt = DateTime.UtcNow;
                    message.LastError = null;
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.LastError = ex.Message;
                    message.NextRetryAt = DateTime.UtcNow.AddSeconds(Math.Min(300, 5 * message.RetryCount));
                    _logger.LogWarning(ex, "Publish OutboxMessage {MessageId} thất bại", message.Id);
                }
            }

            await unitOfWork.SaveChangesAsync();
        }
    }
}
