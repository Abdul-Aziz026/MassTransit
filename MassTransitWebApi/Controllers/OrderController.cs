using MassTransitWebApi.Models;
using MassTransitWebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace MassTransitWebApi.Controllers;

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
    public async Task<ActionResult<OrderResponse>> CreateOrder(CreateOrderRequest orderRequest)
    {
        return Ok(await _orderService.CreateOrderAsync(orderRequest));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(int id)
    {
        try
        {
            return Ok(await _orderService.GetOrderStatusAsync(id));
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"{ex.Message}");
            throw;
        }
    }
}
