using NanoPaymentSystem.Domain;

namespace NanoPaymentSystem.Database;

internal class OutboxEventDto
{
    public long Id { get; init; }

    public Guid PaymentId { get; init; }

    public PaymentStatus Status { get; init; }

    public string Data { get; init; } = string.Empty;
    
    public int RetryCount { get; init; }
}