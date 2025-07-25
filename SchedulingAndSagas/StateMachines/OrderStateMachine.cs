using MassTransit;
using SchedulingAndSagas.Contracts;
using SchedulingAndSagas.Events;
using SchedulingAndSagas.StateInstances;

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

    public State OrderCreated { get; private set; }
    public State StockReserved { get; private set; }
    public State StockReservationFailed { get; private set; }
    public State Notification { get; private set; }


    
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CreateOrderCommand, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => StockReservedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => StockReservationFailedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PaymentCompletedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(m => m.Message.CorrelationId));
        // Initial state transition
        Initially(
            When(CreateOrderCommand)
                .Then (context =>
                {
                    Console.WriteLine($"CreateOrderMessage received in OrderStateMachine: {context.Saga} | OrderCreatedEvent published");
                })
                .Then(context =>
                {
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
                .TransitionTo(OrderCreated)
                .Then(context =>
                {
                    Console.WriteLine(
                        $"CorrelationId: {context.Message.CorrelationId} StockReservedEvent received in OrderStateMachine");
                })
        );

        // Transitions from OrderCreated state
        During (OrderCreated, 
            When(StockReservedEvent)
                .Then(context =>
                {
                    Console.WriteLine($"StockReserved: CorrelationId {context.Saga.CorrelationId}  stock is available ");
                    // context.Saga.CurrentState = "OrderReserved State";
                })
                .Publish( context => new CompletePaymentMessage
                {
                    CorrelationId = context.Saga.CorrelationId,
                    TotalPrice = context.Saga.Price,
                    OrderId = context.Saga.OrderId
                })
                .TransitionTo(StockReserved),
            When(StockReservationFailedEvent)
                .Then(context =>
                {
                    Console.WriteLine($"StockReservation state: {context.Saga.CorrelationId}");
                    context.Saga.CurrentState = "OrderReservationFailed State";
                })
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
        );

        During(StockReserved,
            When(PaymentCompletedEvent)
                .Then(context =>
                {
                    context.Saga.Notification = "Your Order has been confirmed after successful payment!!!";
                    Console.WriteLine($"Payment completed for Order: {context.Saga.OrderId}");
                })
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
                .Finalize() // This completes the saga
        );
        // Define what happens when the saga is completed
        SetCompletedWhenFinalized();
    }
}