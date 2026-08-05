using ECommerce.Contracts.Commands;
using ECommerce.Contracts.Events;
using ECommerce.Messaging.Kafka;
using InventoryService.Application;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
   
        public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
        {
            private readonly IMediator _mediator;
   

            public ProductCreatedConsumer(
                IMediator mediator,
                IInventoryDbContext dbContext)
            {
                _mediator = mediator;
              
            }

            public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
            {
                var message = context.Message;

                try
                {
                   
                    // Send to MediatR (clean separation)
                    var command = new CreateInventoryCommand
                    {
                        ProductId = message.ProductId,
                        StockQuantity = message.StockQuantity
                    };

                    await _mediator.Send(command);
                }
                catch (Exception ex)
                {
                    // IMPORTANT: never crash consumer
                    Console.WriteLine($"Error in ProductCreatedConsumer: {ex.Message}");

                    // In production: log + send to DLQ
                    throw; // optional (MassTransit will retry if configured)
                }
            }
        }
    }
    /*public class ProductCreatedConsumer : KafkaConsumerBase
    {
        private readonly IMediator _mediator;

        public ProductCreatedConsumer(IMediator mediator)
            : base("product.created", "inventory-service")
        {
            _mediator = mediator;
        }

        protected override async Task HandleMessage(string message)
        {
            var evt = JsonSerializer.Deserialize<ProductCreatedEvent>(message);

            if (evt == null)
                return;

            await _mediator.Send(
             new CreateInventoryCommand
             {
                 ProductId = evt.ProductId
             });
        }
    }*/

