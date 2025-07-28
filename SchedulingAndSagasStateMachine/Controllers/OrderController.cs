using MassTransit;
using Microsoft.AspNetCore.Mvc;
using SchedulingAndSagasStateMachine.Contracts;
using SchedulingAndSagasStateMachine.Services;

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
