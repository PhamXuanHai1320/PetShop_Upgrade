using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace PetShop_Upgrade.Messaging
{
    public class PaymentEventConsumerWorker : BackgroundService
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<PaymentEventConsumerWorker> _logger;
        private IConnection? _connection;
        private IChannel? _channel;

        public PaymentEventConsumerWorker(
            IOptions<RabbitMqOptions> options,
            ILogger<PaymentEventConsumerWorker> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunConsumerSessionAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không thể kết nối consumer tới RabbitMQ; sẽ thử lại");
                    await DisposeRabbitMqAsync();
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task RunConsumerSessionAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                ClientProvidedName = "petshop-payment-event-consumer",
                AutomaticRecoveryEnabled = true
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await _channel.ExchangeDeclareAsync(
                _options.ExchangeName, ExchangeType.Topic, durable: true,
                autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(
                _options.QueueName, durable: true, exclusive: false,
                autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(
                _options.QueueName, _options.ExchangeName, "payment.*",
                cancellationToken: stoppingToken);
            await _channel.BasicQosAsync(0, 10, false, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, args) =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(args.Body.Span);
                    _logger.LogInformation(
                        "Đã nhận sự kiện {RoutingKey}: {Payload}", args.RoutingKey, payload);
                    await _channel.BasicAckAsync(args.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Xử lý sự kiện thanh toán thất bại");
                    await _channel.BasicNackAsync(args.DeliveryTag, false, requeue: true, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                _options.QueueName, autoAck: false, consumer: consumer,
                cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await DisposeRabbitMqAsync();
            await base.StopAsync(cancellationToken);
        }

        private async Task DisposeRabbitMqAsync()
        {
            if (_channel != null)
            {
                await _channel.DisposeAsync();
                _channel = null;
            }
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}
