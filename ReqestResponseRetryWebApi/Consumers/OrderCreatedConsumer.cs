using MassTransit;
using MassTransitRequestResponseWebApi2.Contracts.Events;
using MassTransitRequestResponseWebApi2.Extensions;

namespace MassTransitRequestResponseWebApi2.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private static int _attemptCounter = 0;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var order = context.Message;
        var attemptNumber = Interlocked.Increment(ref _attemptCounter);
        var correlationId = context.CorrelationId ?? Guid.NewGuid();

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "🔄 [ORDER-CONSUMER] Processing order {OrderId} (Attempt #{AttemptNumber})",
            order.OrderId, attemptNumber);

        try
        {
            // Simulate processing delay
            await Task.Delay(Random.Shared.Next(500, 1500));

            // Intentionally fail some orders to demonstrate retry behavior
            if (ShouldSimulateFailure(order.OrderId, attemptNumber))
            {
                var errorMessage = GetFailureMessage(order.OrderId, attemptNumber);
                _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                    "❌ [ORDER-CONSUMER] {ErrorMessage}", errorMessage);

                throw new InvalidOperationException(errorMessage);
            }

            // Simulate successful processing
            await ProcessOrderSuccessfully(context, order, correlationId, attemptNumber);
        }
        catch (Exception ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "💥 [ORDER-CONSUMER] Failed to process order {OrderId}: {Error}",
                order.OrderId, ex.Message);

            // Publish failure event for monitoring
            await context.Publish(new OrderFailed
            {
                CorrelationId = correlationId,
                OrderId = order.OrderId,
                ErrorMessage = ex.Message,
                RetryAttempt = attemptNumber
            });

            throw; // Re-throw to trigger retry policy
        }
    }

    private async Task ProcessOrderSuccessfully(ConsumeContext<OrderCreated> context, OrderCreated order, Guid correlationId, int attemptNumber)
    {
        // Simulate business logic
        await ValidateOrder(order, correlationId);
        await ReserveInventory(order, correlationId);
        await ProcessPayment(order, correlationId);

        // Publish success event
        await context.Publish(new OrderProcessed
        {
            CorrelationId = correlationId,
            OrderId = order.OrderId,
            Status = "Processed",
            ProcessingNotes = $"Order processed successfully after {attemptNumber} attempts"
        });

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "✅ [ORDER-CONSUMER] Order {OrderId} processed successfully after {AttemptNumber} attempts",
            order.OrderId, attemptNumber);
    }

    private async Task ValidateOrder(OrderCreated order, Guid correlationId)
    {
        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "🔍 [ORDER-CONSUMER] Validating order {OrderId}", order.OrderId);
        await Task.Delay(200);
    }

    private async Task ReserveInventory(OrderCreated order, Guid correlationId)
    {
        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "📦 [ORDER-CONSUMER] Reserving inventory for order {OrderId}", order.OrderId);
        await Task.Delay(300);
    }

    private async Task ProcessPayment(OrderCreated order, Guid correlationId)
    {
        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "💳 [ORDER-CONSUMER] Processing payment for order {OrderId}", order.OrderId);
        await Task.Delay(400);
    }

    private bool ShouldSimulateFailure(int orderId, int attemptNumber)
    {
        return orderId switch
        {
            // Order 1: Fails first 2 attempts, succeeds on 3rd
            1 => attemptNumber <= 2,
            // Order 2: Always fails (will go to _error queue)
            2 => true,
            // Order 3: Fails only on 1st attempt
            3 => attemptNumber == 1,
            // Order 4: Fails on 1st and 3rd attempts
            4 => attemptNumber == 1 || attemptNumber == 3,
            // Orders 5-10: Random 30% failure rate
            >= 5 and <= 10 => Random.Shared.Next(1, 4) == 1,
            // All other orders: Success
            _ => false
        };
    }

    private string GetFailureMessage(int orderId, int attemptNumber)
    {
        return orderId switch
        {
            1 => $"Temporary network issue for order {orderId} (attempt {attemptNumber})",
            2 => $"Critical system error for order {orderId} - will never succeed",
            3 => $"Database timeout for order {orderId} (attempt {attemptNumber})",
            4 => $"Service unavailable for order {orderId} (attempt {attemptNumber})",
            _ => $"Random processing error for order {orderId} (attempt {attemptNumber})"
        };
    }
}