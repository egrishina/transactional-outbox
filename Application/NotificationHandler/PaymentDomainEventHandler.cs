using MediatR;
using NanoPaymentSystem.Application.Repository;
using NanoPaymentSystem.Domain.DomainEvents;

namespace NanoPaymentSystem.Application.NotificationHandler;

internal class PaymentDomainEventHandler :
    INotificationHandler<PaymentCreatedEvent>,
    INotificationHandler<PaymentProcessingStartedEvent>,
    INotificationHandler<PaymentAuthorizedEvent>,
    INotificationHandler<PaymentRejectedEvent>,
    INotificationHandler<PaymentCancelledEvent>
{
    private readonly IOutboxRepository _outboxRepository;

    public PaymentDomainEventHandler(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    /// <inheritdoc />
    public async Task Handle(PaymentCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _outboxRepository.Save(
            new IntegrationEvent(notification.PaymentId, notification.PaymentStatus, notification.Event),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PaymentProcessingStartedEvent notification, CancellationToken cancellationToken)
    {
        await _outboxRepository.Save(
            new IntegrationEvent(notification.PaymentId, notification.PaymentStatus, notification.Event),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PaymentAuthorizedEvent notification, CancellationToken cancellationToken)
    {
        await _outboxRepository.Save(
            new IntegrationEvent(notification.PaymentId, notification.PaymentStatus, notification.Event),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PaymentRejectedEvent notification, CancellationToken cancellationToken)
    {
        await _outboxRepository.Save(
            new IntegrationEvent(notification.PaymentId, notification.PaymentStatus, notification.Event),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task Handle(PaymentCancelledEvent notification, CancellationToken cancellationToken)
    {
        await _outboxRepository.Save(
            new IntegrationEvent(notification.PaymentId, notification.PaymentStatus, notification.Event),
            cancellationToken);
    }
}
