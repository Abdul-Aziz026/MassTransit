using MassTransit;
using MassTransitRequestResponseWebApi2.Contracts.Commands;
using MassTransitRequestResponseWebApi2.Contracts.Events;
using MassTransitRequestResponseWebApi2.Extensions;
using MassTransitRequestResponseWebApi2.Models;

namespace MassTransitRequestResponseWebApi2.Service;
public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<ProcessOrderResult> ProcessOrderWithValidationAsync(int orderId, string customerEmail);
}

public class OrderService : IOrderService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IRequestClient<ProcessPayment> _paymentClient;
    private readonly IRequestClient<CheckInventory> _inventoryClient;
    private readonly ILogger<OrderService> _logger;
    private static int _orderIdCounter = 1;

    public OrderService(
        IPublishEndpoint publishEndpoint,
        IRequestClient<ProcessPayment> paymentClient,
        IRequestClient<CheckInventory> inventoryClient,
        ILogger<OrderService> logger)
    {
        _publishEndpoint = publishEndpoint;
        _paymentClient = paymentClient;
        _inventoryClient = inventoryClient;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        var orderId = Interlocked.Increment(ref _orderIdCounter);
        var correlationId = Guid.NewGuid();

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "🎫 [ORDER-SERVICE] Creating order {OrderId} for customer {CustomerName}",
            orderId, request.CustomerName);

        var orderCreated = new OrderCreated
        {
            CorrelationId = correlationId,
            OrderId = orderId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            TotalAmount = request.TotalAmount,
            Items = request.Items.Select(item => new Contracts.Events.OrderItem
            {
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        };

        // Publish event with correlation ID
        await _publishEndpoint.PublishWithCorrelation(orderCreated, correlationId);

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "📢 [ORDER-SERVICE] Published OrderCreated event for order {OrderId}", orderId);

        return new OrderResponse
        {
            OrderId = orderId,
            CorrelationId = correlationId,
            Status = "Created",
            CreatedAt = DateTime.UtcNow,
            Message = "Order created and processing started"
        };
    }

    public async Task<ProcessOrderResult> ProcessOrderWithValidationAsync(int orderId, string customerEmail)
    {
        var correlationId = Guid.NewGuid();
        var processingSteps = new List<string>();

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "🔄 [ORDER-SERVICE] Processing order {OrderId} with validation", orderId);

        try
        {
            // Step 1: Check inventory using RequestClient
            processingSteps.Add("Starting inventory check");
            var inventoryResult = await CheckInventoryAsync(orderId, correlationId, processingSteps);

            if (!inventoryResult.IsSuccess)
            {
                return new ProcessOrderResult
                {
                    OrderId = orderId,
                    CorrelationId = correlationId,
                    IsSuccessful = false,
                    Status = "InventoryCheckFailed",
                    Message = inventoryResult.Message,
                    ProcessingSteps = processingSteps
                };
            }

            // Step 2: Process payment using RequestClient
            processingSteps.Add("Starting payment processing");
            var paymentResult = await ProcessPaymentAsync(orderId, customerEmail, 150.00m, correlationId, processingSteps);

            if (!paymentResult.IsSuccess)
            {
                return new ProcessOrderResult
                {
                    OrderId = orderId,
                    CorrelationId = correlationId,
                    IsSuccessful = false,
                    Status = "PaymentFailed",
                    Message = paymentResult.Message,
                    ProcessingSteps = processingSteps
                };
            }

            processingSteps.Add("Order processed successfully");
            return new ProcessOrderResult
            {
                OrderId = orderId,
                CorrelationId = correlationId,
                IsSuccessful = true,
                Status = "Completed",
                Message = "Order processed successfully",
                TransactionId = paymentResult.TransactionId,
                ProcessingSteps = processingSteps
            };
        }
        catch (RequestTimeoutException ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "⏰ [ORDER-SERVICE] Request timeout for order {OrderId}: {Error}", orderId, ex.Message);

            processingSteps.Add($"Request timeout: {ex.Message}");
            return new ProcessOrderResult
            {
                OrderId = orderId,
                CorrelationId = correlationId,
                IsSuccessful = false,
                Status = "Timeout",
                Message = "Request timeout occurred",
                ProcessingSteps = processingSteps
            };
        }
        catch (Exception ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "💥 [ORDER-SERVICE] Error processing order {OrderId}: {Error}", orderId, ex.Message);

            processingSteps.Add($"Processing error: {ex.Message}");
            return new ProcessOrderResult
            {
                OrderId = orderId,
                CorrelationId = correlationId,
                IsSuccessful = false,
                Status = "Error",
                Message = $"Processing error: {ex.Message}",
                ProcessingSteps = processingSteps
            };
        }
    }

    private async Task<(bool IsSuccess, string Message)> CheckInventoryAsync(int orderId, Guid correlationId, List<string> processingSteps)
    {
        var inventoryRequest = new CheckInventory
        {
            CorrelationId = correlationId,
            OrderId = orderId,
            Items = new List<InventoryItem>
            {
                new InventoryItem { ProductName = "Laptop", RequiredQuantity = 1 },
                new InventoryItem { ProductName = "Mouse", RequiredQuantity = 2 }
            }
        };

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "📦 [ORDER-SERVICE] Checking inventory for order {OrderId}", orderId);

        var response = await _inventoryClient.GetResponse<InventoryAvailable, InventoryUnavailable>(inventoryRequest);

        if (response.Is(out Response<InventoryAvailable>? available))
        {
            processingSteps.Add("Inventory check passed - all items available");
            return (true, "All items available");
        }
        else if (response.Is(out Response<InventoryUnavailable>? unavailable))
        {
            processingSteps.Add($"Inventory check failed: {unavailable.Message.Message}");
            return (false, unavailable.Message.Message);
        }

        processingSteps.Add("Inventory check failed - unknown response");
        return (false, "Unknown inventory response");
    }

    private async Task<(bool IsSuccess, string Message, string? TransactionId)> ProcessPaymentAsync(
        int orderId, string customerEmail, decimal amount, Guid correlationId, List<string> processingSteps)
    {
        var paymentRequest = new ProcessPayment
        {
            CorrelationId = correlationId,
            OrderId = orderId,
            Amount = amount,
            CustomerEmail = customerEmail,
            CardNumber = "4111111111111111"
        };

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "💳 [ORDER-SERVICE] Processing payment for order {OrderId}", orderId);

        var response = await _paymentClient.GetResponse<PaymentSuccessful, PaymentFailed>(paymentRequest);

        if (response.Is(out Response<PaymentSuccessful>? successful))
        {
            processingSteps.Add($"Payment successful - Transaction: {successful.Message.TransactionId}");
            return (true, "Payment successful", successful.Message.TransactionId);
        }
        else if (response.Is(out Response<PaymentFailed>? failed))
        {
            processingSteps.Add($"Payment failed: {failed.Message.ErrorMessage}");
            return (false, failed.Message.ErrorMessage, null);
        }

        processingSteps.Add("Payment failed - unknown response");
        return (false, "Unknown payment response", null);
    }
}