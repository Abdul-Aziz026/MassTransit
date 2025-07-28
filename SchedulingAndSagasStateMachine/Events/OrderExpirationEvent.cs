namespace SchedulingAndSagasStateMachine.Events;

public class OrderExpirationEvent
{
    public Guid CorrelationId { get; set; }
    public string OrderId { get; set; }
    public string Email { get; set; }
}