using MassTransit;
using Microsoft.AspNetCore.Mvc;
using SchedulingAndSagas.Contracts;
using SchedulingAndSagas.Services;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMessagePublisher _messagePublisher;
    public OrderController(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder(CreateOrderMessage message)
    {
        await _messagePublisher.PublishAsync(message);
        return Ok();
    }
}

public record CreateOrderRequest(string ProductName);
