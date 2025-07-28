namespace SchedulingAndSagasStateMachine.Events;

public class OrderCreatedEvent
{
    public Guid CorrelationId { get; set; }
    public string OrderId { get; set; }
    public int IsAvailable { get; set; }
}