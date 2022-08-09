using Microsoft.Extensions.Options;
using NanoPaymentSystem.Application.NotificationHandler;
using NanoPaymentSystem.Application.Repository;
using NanoPaymentSystem.Options;

namespace NanoPaymentSystem.Services;

internal class KafkaHostedService : BackgroundService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<KafkaHostedService> _logger;
    private readonly OutboxOptions _outboxOptions;

    public KafkaHostedService(IOutboxRepository outboxRepository, IMessageBroker messageBroker,
        ILogger<KafkaHostedService> logger, IOptions<OutboxOptions> options)
    {
        _outboxRepository = outboxRepository;
        _messageBroker = messageBroker;
        _logger = logger;
        _outboxOptions = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var outboxEvents = await _outboxRepository.GetNewEvents(stoppingToken);
                foreach (var notification in outboxEvents)
                {
                    try
                    {
                        await _messageBroker.Publish(notification, stoppingToken);
                        await _outboxRepository.Remove(notification, stoppingToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning("Failed to send the event to Kafka");
                        await _outboxRepository.IncrementCount(notification, stoppingToken);
                    }
                }

                await Task.Delay(_outboxOptions.RequestIntervalMs, stoppingToken);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.LogWarning("The operation was canceled");
        }
    }
}