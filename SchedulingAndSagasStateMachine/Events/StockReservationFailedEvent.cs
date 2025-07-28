using MassTransit;

namespace SchedulingAndSagasStateMachine.Events;

public class StockReservationFailedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public string OrderId { get; set; }
    public string Reason { get; set; }
}