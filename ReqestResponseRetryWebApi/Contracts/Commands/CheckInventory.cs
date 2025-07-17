namespace MassTransitRequestResponseWebApi2.Contracts.Commands;


// Request
public record CheckInventory
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public int OrderId { get; init; }
    public List<InventoryItem> Items { get; init; } = new();
}

public record InventoryItem
{
    public string ProductName { get; init; } = string.Empty;
    public int RequiredQuantity { get; init; }
}

// Success Response
public record InventoryAvailable
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public List<InventoryItemResult> Items { get; init; } = new();
    public string Message { get; init; } = "All items available";
}

// Error Response
public record InventoryUnavailable
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public List<InventoryItemResult> Items { get; init; } = new();
    public string Message { get; init; } = "Some items not available";
}

public record InventoryItemResult
{
    public string ProductName { get; init; } = string.Empty;
    public int AvailableQuantity { get; init; }
    public int RequiredQuantity { get; init; }
    public bool IsAvailable { get; init; }
}
