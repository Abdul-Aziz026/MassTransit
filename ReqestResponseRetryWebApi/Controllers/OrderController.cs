using MassTransitRequestResponseWebApi2.Extensions;
using MassTransitRequestResponseWebApi2.Models;
using MassTransitRequestResponseWebApi2.Service;
using Microsoft.AspNetCore.Mvc;

namespace MassTransitRequestResponseWebApi2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Create order - Demonstrates event publishing with retry logic
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var correlationId = Guid.NewGuid();

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "🌐 [ORDER-API] Creating order for customer {CustomerName}", request.CustomerName);

        try
        {
            var result = await _orderService.CreateOrderAsync(request);

            return Ok(new ApiResponse<OrderResponse>
            {
                IsSuccess = true,
                Data = result,
                Message = "Order created successfully",
                CorrelationId = result.CorrelationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "🌐 [ORDER-API] Failed to create order: {Error}", ex.Message);

            return StatusCode(500, new ApiResponse<OrderResponse>
            {
                IsSuccess = false,
                Message = "Failed to create order",
                CorrelationId = correlationId,
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Process order with validation - Demonstrates RequestClient usage
    /// </summary>
    [HttpPost("process/{orderId}")]
    public async Task<ActionResult<ApiResponse<ProcessOrderResult>>> ProcessOrder(
        int orderId,
        [FromQuery] string customerEmail = "test@example.com")
    {
        var correlationId = Guid.NewGuid();

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "🌐 [ORDER-API] Processing order {OrderId} with validation", orderId);

        try
        {
            var result = await _orderService.ProcessOrderWithValidationAsync(orderId, customerEmail);

            return Ok(new ApiResponse<ProcessOrderResult>
            {
                IsSuccess = result.IsSuccessful,
                Data = result,
                Message = result.Message,
                CorrelationId = result.CorrelationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "🌐 [ORDER-API] Failed to process order {OrderId}: {Error}", orderId, ex.Message);

            return StatusCode(500, new ApiResponse<ProcessOrderResult>
            {
                IsSuccess = false,
                Message = "Failed to process order",
                CorrelationId = correlationId,
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Demo retry behavior with intentional failures
    /// </summary>
    [HttpPost("demo-retry")]
    public async Task<ActionResult<ApiResponse<List<OrderResponse>>>> DemoRetryBehavior()
    {
        var correlationId = Guid.NewGuid();

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "🌐 [ORDER-API] Starting retry behavior demo");

        try
        {
            var orders = new List<OrderResponse>();

            // Create orders with different failure scenarios
            var scenarios = new List<(string CustomerName, string Scenario)>
            {
                ("Retry Customer 1", "Fails 2 times, succeeds on 3rd attempt"),
                ("Fail Customer 2", "Always fails - will go to _error queue"),
                ("Retry Customer 3", "Fails once, succeeds on retry"),
                ("Retry Customer 4", "Fails randomly during retries")
            };

            foreach (var (customerName, scenario) in scenarios)
            {
                var request = new CreateOrderRequest
                {
                    CustomerName = customerName,
                    CustomerEmail = $"{customerName.Replace(" ", "").ToLower()}@example.com",
                    TotalAmount = 100.00m,
                    Items = new List<CreateOrderItem>
                    {
                        new CreateOrderItem { ProductName = "Laptop", Quantity = 1, Price = 100.00m }
                    }
                };

                var order = await _orderService.CreateOrderAsync(request);
                orders.Add(order);
            }

            return Ok(new ApiResponse<List<OrderResponse>>
            {
                IsSuccess = true,
                Data = orders,
                Message = "Retry demo orders created - check logs and RabbitMQ for retry behavior",
                CorrelationId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "🌐 [ORDER-API] Retry demo failed: {Error}", ex.Message);

            return StatusCode(500, new ApiResponse<List<OrderResponse>>
            {
                IsSuccess = false,
                Message = "Retry demo failed",
                CorrelationId = correlationId,
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Demo request-response patterns
    /// </summary>
    [HttpPost("demo-request-response")]
    public async Task<ActionResult<ApiResponse<List<ProcessOrderResult>>>> DemoRequestResponse()
    {
        var correlationId = Guid.NewGuid();

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "🌐 [ORDER-API] Starting request-response demo");

        try
        {
            var results = new List<ProcessOrderResult>();

            // Test different order IDs with different behaviors
            var orderIds = new[] { 5, 10, 11, 12, 20 }; // Different failure scenarios

            foreach (var orderId in orderIds)
            {
                var result = await _orderService.ProcessOrderWithValidationAsync(orderId, "demo@example.com");
                results.Add(result);
            }

            return Ok(new ApiResponse<List<ProcessOrderResult>>
            {
                IsSuccess = true,
                Data = results,
                Message = "Request-response demo completed - check results for different scenarios",
                CorrelationId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "🌐 [ORDER-API] Request-response demo failed: {Error}", ex.Message);

            return StatusCode(500, new ApiResponse<List<ProcessOrderResult>>
            {
                IsSuccess = false,
                Message = "Request-response demo failed",
                CorrelationId = correlationId,
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("{orderId}")]
    public ActionResult<ApiResponse<object>> GetOrder(int orderId)
    {
        return Ok(new ApiResponse<object>
        {
            IsSuccess = true,
            Data = new { OrderId = orderId, Status = "Retrieved" },
            Message = "Order retrieved successfully",
            CorrelationId = Guid.NewGuid()
        });
    }
}