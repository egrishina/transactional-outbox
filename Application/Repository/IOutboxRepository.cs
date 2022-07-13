using NanoPaymentSystem.Application.NotificationHandler;

namespace NanoPaymentSystem.Application.Repository;

public interface IOutboxRepository
{
    Task Save(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
    
    Task<IEnumerable<IntegrationEvent>> GetNewEvents(CancellationToken cancellationToken);
    
    Task Remove(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
    
    Task IncrementCount(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
}