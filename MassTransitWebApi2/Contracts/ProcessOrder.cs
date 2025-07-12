namespace MassTransitRequestResponseWebApi2.Contracts;


// Command - Request
public record ProcessOrder
{
    public int OrderId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string Priority { get; init; } = "Normal";
    public decimal TotalAmount { get; init; }
}

// Response
public record OrderProcessingResult
{
    public int OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}