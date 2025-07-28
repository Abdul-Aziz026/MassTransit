namespace SchedulingAndSagasStateMachine.Contracts;

public class CompletePaymentMessage
{
    public Guid CorrelationId { get; set; }
    public decimal TotalPrice { get; set; }
    public string OrderId { get; set; }
}
