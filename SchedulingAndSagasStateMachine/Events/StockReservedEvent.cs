using MassTransit;

namespace SchedulingAndSagasStateMachine.Events;

public class StockReservedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public string OrderId { get; set; }
}