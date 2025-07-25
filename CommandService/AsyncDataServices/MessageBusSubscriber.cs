using System.Text;
using System.Threading.Tasks;
using CommandService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandService.AsyncDataServices;

public class MessageBusSubscriber : BackgroundService
{
    private readonly string _exchangeName = "trigger";
    private readonly ILogger<MessageBusSubscriber> _logger;
    private readonly IEventProcessor _eventProcessor;
    private readonly IConnection? _connection;
    private readonly IChannel? _channel;
    private bool _disposed = false;
    private string _queueName;

    public MessageBusSubscriber(
        IConfiguration configuration,
        IEventProcessor eventProcessor,
        ILogger<MessageBusSubscriber> logger
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventProcessor = eventProcessor ?? throw new ArgumentNullException(nameof(eventProcessor));

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQHost"] ?? "localhost",
            Port = int.TryParse(configuration["RabbitMQPort"], out int port) ? port : 5672,
            UserName = configuration["RabbitMQUser"] ?? "guest",
            Password = configuration["RabbitMQPassword"] ?? "guest",
            VirtualHost = configuration["RabbitMQVirtualHost"] ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // Declare exchange
            _channel
                .ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Fanout)
                .GetAwaiter()
                .GetResult();

            _queueName = _channel.QueueDeclareAsync().GetAwaiter().GetResult().QueueName;
            _channel.QueueBindAsync(_queueName, _exchangeName, "");

            _logger.LogInformation("--> Listening on the Message bus");

            _connection.ConnectionShutdownAsync += OnConnectionShutdown;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not connect to the message bus: {Message}", ex.Message);
            throw;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += async (ModuleHandle, ea) =>
        {
            _logger.LogInformation("--> Event Received");
            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            _eventProcessor.ProcessEvent(notificationMessage);
            await Task.CompletedTask;
        };

        _channel?.BasicConsumeAsync(_queueName, true, consumer, stoppingToken);

        return Task.CompletedTask;
    }

    private Task OnConnectionShutdown(object sender, ShutdownEventArgs args)
    {
        _logger.LogInformation("--> Connection shut down: {0}", args.ReplyText);
        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                _channel?.CloseAsync().GetAwaiter().GetResult();
                _connection?.CloseAsync().GetAwaiter().GetResult();
                _channel?.Dispose();
                _connection?.Dispose();
                _logger.LogInformation("MessageBusSubscriber disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing MessageBusSubscriber: {Message}", ex.Message);
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
