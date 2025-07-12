using MassTransit;
using MassTransit.Transports;
using MassTransitWebApi.Contracts;
using MassTransitWebApi.Models;

namespace MassTransitWebApi.Service;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse> GetOrderStatusAsync(int id);
}

public class OrderService : IOrderService
{
    public readonly IPublishEndpoint publishEndpoint;
    public readonly ILogger<OrderService> logger;
    public static List<OrderResponse> OrdersList = new List<OrderResponse>();

    public OrderService(IPublishEndpoint publishEndpoint, ILogger<OrderService> logger)
    {
        this.publishEndpoint = publishEndpoint;
        this.logger = logger;
    }
    
    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            // create order message...
            var orderCreated = new OrderCreated()
            {
                Id = request.Id,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                TotalAmount = request.TotalAmount
            };
            // store order for tracking...
            var orderResponse = new OrderResponse()
            {
                Id = request.Id,
                Status = "Created",
                Message = "Order Processed by service..."
            };
            OrdersList.Add(orderResponse);

            // Publish the message
            await publishEndpoint.Publish(orderCreated);
            logger.LogInformation($"Order {request.Id}Created and publish for consumer");
            return orderResponse;
        }
        catch (Exception ex)
        {
            logger.LogInformation($"Error occur: {ex.Message}");
            throw;
        }
    }

    public async Task<OrderResponse> GetOrderStatusAsync(int id)
    {
        await Task.CompletedTask; // Simulate async operation

        if (OrdersList.Count > id)
        {
            return OrdersList[id];
        }

        throw new KeyNotFoundException($"Order {id} not found");
    }
}