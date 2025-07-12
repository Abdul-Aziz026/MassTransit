namespace MassTransitWebApi.Contracts;

public record OrderCreated
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
}
