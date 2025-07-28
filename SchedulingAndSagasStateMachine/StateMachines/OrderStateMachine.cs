using MassTransit;
using SchedulingAndSagasStateMachine.Contracts;
using SchedulingAndSagasStateMachine.Events;
using SchedulingAndSagasStateMachine.StateInstances;

namespace StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    // commands
    private Event<CreateOrderMessage> CreateOrderCommand { get; set; }

    // events
    private Event<StockReservedEvent> StockReservedEvent { get; set; }
    private Event<StockReservationFailedEvent> StockReservationFailedEvent { get; set; }
    private Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
    private Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }


    // Schedules
    private Schedule<OrderState, PaymentTimeoutEvent> PaymentTimeout { get; set; }
    private Schedule<OrderState, OrderExpirationEvent> OrderExpiration { get; set; }

    public State OrderCreated { get; private set; }
    public State StockReserved { get; private set; }
    public State StockReservationFailed { get; private set; }
    public State Notification { get; private set; }
    public State PaymentTimedOut { get; private set; }
    public State OrderExpired { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CreateOrderCommand, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => StockReservedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => StockReservationFailedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PaymentCompletedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));

        // Configure schedules
        Schedule(() => PaymentTimeout, x => x.PaymentTimer, s =>
        {
            s.Delay = TimeSpan.FromSeconds(5); // 5 second payment timeout
            s.Received = r => r.CorrelateById(m => m.Message.CorrelationId);
        });

        Schedule(() => OrderExpiration, x => x.ExpirationTimer, s =>
        {
            // s.Delay = TimeSpan.FromHours(24); // 24 hours order expiration
            s.Delay = TimeSpan.FromSeconds(5); // 5 second order expiration
            s.Received = r => r.CorrelateById(m => m.Message.CorrelationId);
        });


        // Initial state transition
        Initially(
            When(CreateOrderCommand)
                .Then(context =>
                {
                    Console.WriteLine($"CreateOrderCommand received for OrderId: {context.Message.OrderId}");
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.Price = context.Message.Price;
                    context.Saga.CreatedDate = context.Message.CreatedDate;
                    context.Saga.Quantity = context.Message.Quantity;
                    context.Saga.Email = context.Message.Email;
                })
                .Publish(context => new OrderCreatedEvent()
                {
                    CorrelationId = context.Saga.CorrelationId,
                    IsAvailable = context.Saga.Quantity,
                    OrderId = context.Saga.OrderId,
                })

                // Schedule order expiration timeout
                .Schedule(OrderExpiration, context => new OrderExpirationEvent
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Email = context.Saga.Email
                })
                .TransitionTo(OrderCreated)
        );

        // Transitions from OrderCreated state
        During (OrderCreated, 
            When(StockReservedEvent)
                .Then(context =>
                {
                    Console.WriteLine($"StockReserved: CorrelationId {context.Saga.CorrelationId}  stock is available ");
                })
                .Publish( context => new CompletePaymentMessage
                {
                    CorrelationId = context.Saga.CorrelationId,
                    TotalPrice = context.Saga.Price,
                    OrderId = context.Saga.OrderId
                })
                .Schedule(PaymentTimeout, context => new PaymentTimeoutEvent
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Email = context.Saga.Email
                })
                .TransitionTo(StockReserved),
            When(StockReservationFailedEvent)
                .Then(context =>
                {
                    Console.WriteLine($"Stock reservation failed for CorrelationId: {context.Saga.CorrelationId}");
                    context.Saga.Notification = "Your order has been cancelled due to stock unavailability.";
                })
                .Unschedule(OrderExpiration)
                .TransitionTo(StockReservationFailed)
                .Publish(context => new OrderFailedEvent
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Email = context.Saga.Email,
                    ErrorMessage = "Failed Order"
                })
                .Then (context => 
                {
                    Console.WriteLine ("Send failed event..."); 
                })
                .Finalize(),
            // ADD THIS: Handle order expiration while in OrderCreated state 
            When(OrderExpiration?.Received)
                .Then(context => 
                {
                    Console.WriteLine($"Order expired for OrderId: {context.Saga.OrderId}"); 
                    context.Saga.Notification = "Your order has expired due to inactivity."; 
                })
                .Publish(context => new OrderFailedEvent 
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Email = context.Saga.Email,
                    ErrorMessage = "Order Expired - No stock reservation activity"
                })
                .TransitionTo(OrderExpired)
                .Finalize()
            );

        During(StockReserved,
            When(PaymentCompletedEvent)
                .Then(context =>
                {
                    context.Saga.Notification = "Your Order has been confirmed after successful payment!!!";
                    Console.WriteLine($"Payment completed for Order: {context.Saga.OrderId}");
                })
                .Unschedule(PaymentTimeout)
                .Unschedule(OrderExpiration)
                .Publish(context => new Notification()
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Email = context.Saga.Email,
                    Message = context.Saga.Notification
                })
                .TransitionTo(Notification)
                .Then(context =>
                {
                    Console.WriteLine($"NotificationEvent published and transitioned to Notification state for Order: {context.Saga.OrderId}");
                    Console.WriteLine($"Saga will now be completed");
                })
                .Finalize(), // This completes the saga

            When(PaymentFailedEvent)
                .Then(context =>
                {
                    context.Saga.Notification = "Your Order has been cancelled due to payment failure!!!";
                    Console.WriteLine($"Payment failed for Order: {context.Saga.OrderId}");
                })
                .Unschedule(PaymentTimeout)
                .Unschedule(OrderExpiration)
                .Publish(context => new Notification()
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Email = context.Saga.Email,
                    Message = context.Saga.Notification
                })
                .TransitionTo(Notification)
                .Then(context =>
                {
                    Console.WriteLine($"NotificationEvent published and transitioned to Notification state for Order: {context.Saga.OrderId}");
                    Console.WriteLine($"Saga will now be completed");
                })
                .Finalize(),
            When(PaymentTimeout?.Received)
                .Then(context =>
                {
                    Console.WriteLine($"Payment timed out for Order: {context.Saga.OrderId}");
                    context.Saga.Notification = "Your order has been cancelled due to payment timeout.";
                })
                .Unschedule(OrderExpiration)
                .Publish(context => new OrderFailedEvent
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    Email = context.Saga.Email,
                    ErrorMessage = "Payment Timeout"
                })
                .TransitionTo(PaymentTimedOut)
                .Finalize()
        );
        // Define what happens when the saga is completed
        SetCompletedWhenFinalized();
    }
}