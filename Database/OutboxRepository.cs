using Dapper;
using NanoPaymentSystem.Application.NotificationHandler;
using NanoPaymentSystem.Application.Repository;
using NanoPaymentSystem.Domain;
using NanoPaymentSystem.Domain.LifecycleData;
using Newtonsoft.Json;
using Npgsql;

namespace NanoPaymentSystem.Database;

internal class OutboxRepository : IOutboxRepository
{
    private const int RetryLimit = 10;
    private readonly string _connectionString;

    private const string InsertCommand =
        @"insert into payment_outbox (id, entity_id, status, data, retry_count)
          values (DEFAULT, @paymentId, @status, @data, @retryCount);";

    private const string SelectCommand =
        @"select id, entity_id as paymentId, status, data, retry_count as retryCount
          from payment_outbox
          where retry_count < @retryLimit";

    private const string DeleteCommand =
        @"delete from payment_outbox
          where entity_id = @paymentId AND status = @status AND data = @data";

    private const string UpdateCommand =
        @"update payment_outbox
          set retry_count = (select retry_count from payment_outbox where entity_id = @paymentId AND status = @status AND data = @data) + 1";

    public OutboxRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task Save(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(InsertCommand, new
        {
            paymentId = integrationEvent.PaymentId,
            status = integrationEvent.Status,
            data = JsonConvert.SerializeObject(integrationEvent.Data),
            retryCount = 0
        });
    }

    public async Task<IEnumerable<IntegrationEvent>> GetNewEvents(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var retryLimit = RetryLimit;
        var outboxEvents = await connection.QueryAsync<OutboxEventDto>(SelectCommand, new { retryLimit });
        
        var integrationEvents = outboxEvents.Select(dto =>
        {
            IntegrationEvent integrationEvent = dto.Status switch
            {
                PaymentStatus.New => new IntegrationEvent(dto.PaymentId, dto.Status,
                    JsonConvert.DeserializeObject<PaymentCreatedData>(dto.Data)!),

                PaymentStatus.Processing => new IntegrationEvent(dto.PaymentId, dto.Status,
                    JsonConvert.DeserializeObject<PaymentProcessingStartedData>(dto.Data)!),

                PaymentStatus.Authorized => new IntegrationEvent(dto.PaymentId, dto.Status,
                    JsonConvert.DeserializeObject<PaymentAuthorizedData>(dto.Data)!),

                PaymentStatus.Rejected => new IntegrationEvent(dto.PaymentId, dto.Status,
                    JsonConvert.DeserializeObject<PaymentRejectedData>(dto.Data)!),

                PaymentStatus.Cancelled => new IntegrationEvent(dto.PaymentId, dto.Status,
                    JsonConvert.DeserializeObject<PaymentCanceledData>(dto.Data)!),

                PaymentStatus.CancelRequested => new IntegrationEvent(dto.PaymentId, dto.Status,
                    JsonConvert.DeserializeObject<PaymentRequestCancelData>(dto.Data)!),

                _ => throw new ArgumentOutOfRangeException()
            };

            return integrationEvent;
        });

        return integrationEvents;
    }

    public async Task Remove(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(DeleteCommand, new
        {
            paymentId = integrationEvent.PaymentId,
            status = integrationEvent.Status,
            data = JsonConvert.SerializeObject(integrationEvent.Data)
        });
    }

    public async Task IncrementCount(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(UpdateCommand, new
        {
            paymentId = integrationEvent.PaymentId,
            status = integrationEvent.Status,
            data = JsonConvert.SerializeObject(integrationEvent.Data)
        });
    }
}