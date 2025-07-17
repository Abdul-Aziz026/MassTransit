using MassTransit;
using MassTransitRequestResponseWebApi2.Contracts.Commands;
using MassTransitRequestResponseWebApi2.Extensions;

namespace MassTransitRequestResponseWebApi2.Consumers;


public class InventoryConsumer : IConsumer<CheckInventory>
{
    private readonly ILogger<InventoryConsumer> _logger;

    // Mock inventory data
    private static readonly Dictionary<string, int> _inventory = new()
    {
        { "Laptop", 10 },
        { "Mouse", 50 },
        { "Keyboard", 25 },
        { "Monitor", 5 },
        { "Headphones", 15 }
    };

    public InventoryConsumer(ILogger<InventoryConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CheckInventory> context)
    {
        var request = context.Message;
        var correlationId = context.CorrelationId ?? request.CorrelationId;

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "📦 [INVENTORY-CONSUMER] Checking inventory for order {OrderId}", request.OrderId);

        try
        {
            // Simulate inventory check delay
            await Task.Delay(Random.Shared.Next(300, 800));

            // Intentionally fail some inventory checks
            if (ShouldSimulateInventoryFailure(request.OrderId))
            {
                var errorMessage = $"Inventory system unavailable for order {request.OrderId}";
                _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                    "❌ [INVENTORY-CONSUMER] {ErrorMessage}", errorMessage);

                throw new InvalidOperationException(errorMessage);
            }

            var results = new List<InventoryItemResult>();
            var allAvailable = true;

            foreach (var item in request.Items)
            {
                var available = _inventory.TryGetValue(item.ProductName, out var stock) ? stock : 0;
                var isAvailable = available >= item.RequiredQuantity;

                if (!isAvailable)
                    allAvailable = false;

                results.Add(new InventoryItemResult
                {
                    ProductName = item.ProductName,
                    AvailableQuantity = available,
                    RequiredQuantity = item.RequiredQuantity,
                    IsAvailable = isAvailable
                });

                _logger.LogWithCorrelation(LogLevel.Information, correlationId,
                    "📦 [INVENTORY-CONSUMER] {ProductName}: Available={Available}, Required={Required}, Status={Status}",
                    item.ProductName, available, item.RequiredQuantity, isAvailable ? "OK" : "INSUFFICIENT");
            }

            if (allAvailable)
            {
                await context.RespondAsync(new InventoryAvailable
                {
                    CorrelationId = correlationId,
                    OrderId = request.OrderId,
                    Items = results
                });

                _logger.LogWithCorrelation(LogLevel.Information, correlationId,
                    "✅ [INVENTORY-CONSUMER] All items available for order {OrderId}", request.OrderId);
            }
            else
            {
                await context.RespondAsync(new InventoryUnavailable
                {
                    CorrelationId = correlationId,
                    OrderId = request.OrderId,
                    Items = results,
                    Message = "Some items are not available"
                });

                _logger.LogWithCorrelation(LogLevel.Warning, correlationId,
                    "⚠️ [INVENTORY-CONSUMER] Some items unavailable for order {OrderId}", request.OrderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "💥 [INVENTORY-CONSUMER] Inventory check failed for order {OrderId}: {Error}",
                request.OrderId, ex.Message);

            throw; // Re-throw to trigger retry policy
        }
    }

    private bool ShouldSimulateInventoryFailure(int orderId)
    {
        return orderId switch
        {
            // Order 20: Always fails inventory check
            20 => true,
            // Orders 21-25: Random 20% failure rate
            >= 21 and <= 25 => Random.Shared.Next(1, 6) == 1,
            // All other orders: Success
            _ => false
        };
    }
}