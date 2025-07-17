namespace MassTransitRequestResponseWebApi2.Models;
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> Errors { get; set; } = new();
}

public class ProcessOrderResult
{
    public int OrderId { get; set; }
    public Guid CorrelationId { get; set; }
    public bool IsSuccessful { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public List<string> ProcessingSteps { get; set; } = new();
}