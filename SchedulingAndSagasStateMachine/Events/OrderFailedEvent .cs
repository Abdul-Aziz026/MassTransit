using MassTransit;

namespace SchedulingAndSagasStateMachine.Events;


public class OrderFailedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; } // Required for saga correlation
    public string OrderId { get; set; }
    public string Email { get; set; }
    public string ErrorMessage { get; set; }
}