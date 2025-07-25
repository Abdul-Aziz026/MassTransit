using MassTransit;

namespace SchedulingAndSagas.StateInstances;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string CurrentState { get; set; } = null!;
    public string OrderId { get; set; }
    public string Email { get; set; }
    public string Notification { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
}
