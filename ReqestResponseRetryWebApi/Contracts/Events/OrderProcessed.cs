
using MassTransit;

namespace MassTransitRequestResponseWebApi2.Contracts.Events;

public record OrderProcessed : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
    public string ProcessingNotes { get; init; } = string.Empty;
}

