using System.ComponentModel.DataAnnotations;

namespace MassTransitRequestResponseWebApi2.Models;

public class ProcessPaymentRequest
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "CreditCard";

    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
}

public class PaymentResponse
{
    public int OrderId { get; set; }
    public bool IsSuccessful { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class PaymentStatusRequest
{
    [Required]
    public string TransactionId { get; set; } = string.Empty;
}

public record PaymentStatusResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}