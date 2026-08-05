using ECommerce.Contracts.Commands;
using ECommerce.Contracts.Events;
using ECommerce.Messaging.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Confluent.Kafka.ConfigPropertyNames;

namespace OrderService.Application.Sagas.Activities
{
    public class OrderSagaActivities
    {
        private readonly ITopicProducer<ReserveInventoryCommand> _producer;


        public OrderSagaActivities(ITopicProducer<ReserveInventoryCommand> producer)
        {
            _producer = producer;

        }
        public async Task SendReserveInventoryCommands(BehaviorContext<OrderState, OrderCreatedEvent> context)
        {
            foreach (var item in context.Message.Items)
            {
                await _producer.Produce(new ReserveInventoryCommand
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Message.OrderId,
                    ProductId = item.ProductId,
                    StockQuantity = item.Quantity
                });
               
            }
        }
        public async Task ConfirmOrder(BehaviorContext<OrderState> context)
        {
            var endpoint = await context.GetSendEndpoint(new Uri("queue:confirm-order"));

            await endpoint.Send(new ConfirmOrderCommand(
                context.Saga.CorrelationId,
                context.Saga.OrderId));
        }
        public async Task CancelOrder(BehaviorContext<OrderState> context)
        {
            var endpoint = await context.GetSendEndpoint(new Uri("queue:cancel-order"));

            await endpoint.Send(new CancelOrderCommand(
                context.Saga.CorrelationId,
                context.Saga.OrderId,
                "Inventory rejected"));
        }
    }
}
