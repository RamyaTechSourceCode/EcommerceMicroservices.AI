using ECommerce.Contracts.Commands;
using ECommerce.Contracts.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Sagas.Activities
{
    public class ReserveInventoryActivity :
    IStateMachineActivity<OrderState, OrderCreatedEvent>
    {
        private readonly ITopicProducer<ReserveInventoryCommand> _producer;

        public ReserveInventoryActivity(
            ITopicProducer<ReserveInventoryCommand> producer)
        {
            _producer = producer;
        }

        public async Task Execute(
            BehaviorContext<OrderState, OrderCreatedEvent> context,
            IBehavior<OrderState, OrderCreatedEvent> next)
        {
            foreach (var item in context.Message.Items)
            {
                await _producer.Produce(new ReserveInventoryCommand
                {
                    CorrelationId = context.Saga.CorrelationId,
                    OrderId = context.Saga.OrderId,
                    ProductId = item.ProductId,
                    StockQuantity = item.Quantity
                });
            }

            await next.Execute(context);
        }

        public Task Faulted<TException>(
            BehaviorExceptionContext<OrderState, OrderCreatedEvent, TException> context,
            IBehavior<OrderState, OrderCreatedEvent> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context) { }

        public void Accept(StateMachineVisitor visitor) { }
    }
}
