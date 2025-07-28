using MassTransit;
namespace SchedulingAndSagasStateMachine.Contracts;

public class CreateOrderMessage : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public string OrderId { get; set; }
    public string Email { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set;}

}