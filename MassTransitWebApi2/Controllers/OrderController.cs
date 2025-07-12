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

    [HttpPost("create")]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(request);
        return Ok(result);
    }

    [HttpPost("process/{orderId}")]
    public async Task<ActionResult> ProcessOrder(int orderId, [FromQuery] string priority = "Normal")
    {
        var result = await _orderService.ProcessOrderDirectAsync(orderId, priority);

        return Ok(new
            {
                orderId = result.OrderId,
                status = result.Status,
                message = result.Message,
                processedAt = result.ProcessedAt,
                pattern = "REQUEST-RESPONSE (IRequestClient)"
            });
    }
}
