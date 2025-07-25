using MassTransit;

namespace SchedulingAndSagas.Events;

public class StockReservedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public string OrderId { get; set; }
}