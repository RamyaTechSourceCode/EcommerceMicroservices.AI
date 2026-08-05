using Automatonymous;
using ECommerce.Contracts.Commands;
using ECommerce.Contracts.Events;
using MassTransit;
using OrderService.Application.Sagas.Activities;
using static Confluent.Kafka.ConfigPropertyNames;

namespace OrderService.Application.Sagas
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Pending { get; private set; }
        public State ReservingInventory { get; private set; }
        public State Completed { get; private set; }
        public State Cancelled { get; private set; }

        public Event<OrderCreatedEvent> OrderSubmitted { get; private set; } = null!;
        public Event<InventoryReservedEvent> InventoryReserved { get; private set; }
        public Event<InventoryRejectedEvent> InventoryReservationFailed { get; private set; }
        // private readonly OrderSagaActivities _activities;

        /*private static bool IsCompleted(OrderState saga)
        {
            return saga.ReservedItems == saga.TotalItems;
        }*/

        public OrderStateMachine()
        {

        InstanceState(x => x.CurrentState);

            Event(() => OrderSubmitted, x =>
            {
                x.CorrelateById(m => m.Message.CorrelationId);
                x.InsertOnInitial = true;

                x.SetSagaFactory(context => new OrderState
                {
                    CorrelationId = context.Message.CorrelationId,
                     OrderId = context.Message.OrderId
                     
                });
            });
            Event(() => InventoryReserved, x =>
                x.CorrelateById(m => m.Message.CorrelationId));

            Event(() => InventoryReservationFailed, x =>
                x.CorrelateById(m => m.Message.CorrelationId));

            Initially(
                When(OrderSubmitted)

                    .Then(context =>
                    {
                            Console.WriteLine($"Received OrderSubmitted {context.Message.OrderId}");
                        
                          //   throw new Exception("Saga reached this point");
                      
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.TotalItems = context.Message.Items.Count;
                        //context.Saga.ReservedItems = 0;
                        //context.Saga.FailedItems = 0;
          
                        context.Saga.PendingProducts =
                            context.Message.Items.Select(i => i.ProductId).ToList();
                    })
                    .Activity(x => x.OfType<ReserveInventoryActivity>()
                    .TransitionTo(ReservingInventory)
                    )
            );
            //test

            DuringAny(
    When(InventoryReserved)
        .Then(x =>
        {
            Console.WriteLine(
                $"InventoryReserved received: {x.Message.CorrelationId}");
        })
);

            During(ReservingInventory,

                When(InventoryReserved)
                    .Then(context =>
                    {
                        Console.WriteLine(
        $"InventoryReserved received for ProductId {context.Message.ProductId}");

                       // context.Saga.ReservedItems++;
                        context.Saga.PendingProducts.Remove(context.Message.ProductId);
                        context.Saga.ReservedProducts.Add(context.Message.ProductId);
                    })
                     .If(ctx => !ctx.Saga.PendingProducts.Any(),
                     //.If(context => !context.Saga.PendingProducts.Any(),
                    x => x.ThenAsync(async context =>
                    {
                        await context.Publish(new ConfirmOrderCommand
                        (
                             context.Saga.CorrelationId,
                             context.Saga.OrderId
                          
                        ));
                    })
                    .TransitionTo(Completed)
                ),

            When(InventoryReservationFailed)
                   .Then(context =>
                   {
                       //context.Saga.FailedItems++;
                   })
                    .ThenAsync(async context =>
                    {
                        await context.Publish(new CancelOrderCommand
                        (
                             context.Saga.CorrelationId,
                             context.Saga.OrderId,
                             "Insufficient stock"
                        ));
                    })
                    .TransitionTo(Cancelled)
            );
        }
    }
}