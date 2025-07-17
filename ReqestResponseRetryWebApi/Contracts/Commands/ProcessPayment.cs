namespace MassTransitRequestResponseWebApi2.Contracts.Commands;

// Request
public record ProcessPayment
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public int OrderId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = "CreditCard";
    public string CustomerEmail { get; init; } = string.Empty;
    public string CardNumber { get; init; } = string.Empty;
}

// Success Response
public record PaymentSuccessful
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
    public string Status { get; init; } = "Completed";
}

// Error Response
public record PaymentFailed
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public string ErrorCode { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; } = DateTime.UtcNow;
}
