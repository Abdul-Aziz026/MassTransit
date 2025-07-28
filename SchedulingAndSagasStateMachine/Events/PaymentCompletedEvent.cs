using MassTransit;

namespace SchedulingAndSagasStateMachine.Events;

public class PaymentCompletedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CompletedAt { get; set; }
}