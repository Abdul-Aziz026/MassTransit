using MassTransit;
using MassTransit.Transports;
using MassTransitRequestResponseWebApi2.Contracts;
using MassTransitRequestResponseWebApi2.Models;

namespace MassTransitRequestResponseWebApi2.Service;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderProcessingResult> ProcessOrderDirectAsync(int orderId, string priority);
}

public class OrderService : IOrderService
{
    private readonly IPublishEndpoint _publishEndpoint; // For Events (Publish)
    private readonly IRequestClient<ProcessOrder> _requestClient; // For Commands (Request-Response)
    private readonly ILogger<OrderService> _logger;
    private static int _orderIdCounter = 1;

    public OrderService(IPublishEndpoint publishEndpoint,
        IRequestClient<ProcessOrder> requestClient,
        ILogger<OrderService> logger)
    {
        _publishEndpoint = publishEndpoint;
        _requestClient = requestClient;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        var orderId = Interlocked.Increment(ref _orderIdCounter);

        var orderCreated = new OrderCreated
        {
            Id = orderId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            TotalAmount = request.TotalAmount
        };

        // PUBLISH Event - Multiple consumers will receive this
        await _publishEndpoint.Publish(orderCreated);
        _logger.LogInformation("📢 PUBLISHED OrderCreated event for Order {OrderId}", orderId);

        return new OrderResponse
        {
            Id = orderId,
            Status = "Created",
            Message = "Order created and published to all subscribers"
        };
    }

    public async Task<OrderProcessingResult> ProcessOrderDirectAsync(int orderId, string priority)
    {
        var processCommand = new ProcessOrder
        {
            OrderId = orderId,
            CustomerName = $"Customer-{orderId}",
            Priority = priority,
            TotalAmount = 100.00m
        };

        try
        {
            // REQUEST-RESPONSE - Wait for specific response
            var response = await _requestClient.GetResponse<OrderProcessingResult>(processCommand);

            _logger.LogInformation("📞 RECEIVED response for Order {OrderId}: {Status}",
                orderId, response.Message.Status);

            return response.Message;
        }
        catch (RequestTimeoutException)
        {
            _logger.LogWarning("⏰ Request timeout for Order {OrderId}", orderId);
            return new OrderProcessingResult
            {
                OrderId = orderId,
                Status = "Timeout",
                Message = "Processing request timed out"
            };
        }
    }
}