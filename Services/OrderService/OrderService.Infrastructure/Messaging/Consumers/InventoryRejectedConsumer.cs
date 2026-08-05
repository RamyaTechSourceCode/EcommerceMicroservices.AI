using ECommerce.Contracts.Events;
using MassTransit;
using MediatR;
using OrderService.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class InventoryRejectedConsumer : IConsumer<InventoryRejectedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public InventoryRejectedConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<InventoryRejectedEvent> context)
        {
            await _publishEndpoint.Publish(context.Message);
        }
        /* private readonly IMediator _mediator;

         public InventoryRejectedConsumer(IMediator mediator)
         {
             Console.WriteLine("🔥 Consumer CREATED");

             _mediator = mediator;
         }

         public async Task Consume(ConsumeContext<InventoryRejectedEvent> context)
         {
             await _mediator.Send(new CancelOrderCommand(
                 context.Message.CorrelationId,
                 context.Message.OrderId,
                 context.Message.Reason));
         }*/
    }
}
