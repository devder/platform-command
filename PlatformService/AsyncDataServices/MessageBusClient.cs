using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlatformService.AsyncDataServices;

public class MessageBusClient : IMessageBusClient, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageBusClient> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName = "trigger";
    private bool _disposed = false;

    public MessageBusClient(IConfiguration configuration, ILogger<MessageBusClient> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQHost"] ?? "localhost",
            Port = int.TryParse(_configuration["RabbitMQPort"], out int port) ? port : 5672,
            UserName = _configuration["RabbitMQUser"] ?? "guest",
            Password = _configuration["RabbitMQPassword"] ?? "guest",
            VirtualHost = _configuration["RabbitMQVirtualHost"] ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult(); // to handle await ops where async does not work
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // Declare exchange
            _channel
                .ExchangeDeclareAsync(
                    exchange: _exchangeName,
                    type: ExchangeType.Fanout,
                    durable: false,
                    autoDelete: false
                )
                .GetAwaiter()
                .GetResult();

            _connection.ConnectionShutdownAsync += OnConnectionShutdown;
            _logger.LogInformation("Connected to MessageBus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to the message bus: {Message}", ex.Message);
            throw;
        }
    }

    public async Task PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(MessageBusClient));
        ArgumentNullException.ThrowIfNull(platformPublishedDto);

        var message = JsonSerializer.Serialize(platformPublishedDto);
        await PublishMessageAsync(message);
    }

    public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
    {
        PublishNewPlatformAsync(platformPublishedDto).GetAwaiter().GetResult();
    }

    private async Task PublishMessageAsync(string message)
    {
        if (!_connection.IsOpen || _disposed)
        {
            _logger.LogWarning("RabbitMQ connection is not open, cannot send message");
            return;
        }

        try
        {
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: _exchangeName,
                routingKey: string.Empty,
                body: body
            );

            _logger.LogInformation("Message sent: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message: {Message}", ex.Message);
            throw;
        }
    }

    private Task OnConnectionShutdown(object sender, ShutdownEventArgs args)
    {
        _logger.LogInformation("RabbitMQ connection shutdown: {Reason}", args.ReplyText);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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
                _logger.LogInformation("MessageBusClient disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing MessageBusClient: {Message}", ex.Message);
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    ~MessageBusClient()
    {
        Dispose(false);
    }
}
