using MassTransit;

namespace MassTransitRequestResponseWebApi2.Contracts.Events;


public record OrderCreated : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public int OrderId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public List<OrderItem> Items { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string Source { get; init; } = "API";
}

public record OrderItem
{
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}
