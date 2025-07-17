using MassTransit;

namespace MassTransitRequestResponseWebApi2.Contracts.Events;

public record OrderFailed : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; } = DateTime.UtcNow;
    public int RetryAttempt { get; init; }
}