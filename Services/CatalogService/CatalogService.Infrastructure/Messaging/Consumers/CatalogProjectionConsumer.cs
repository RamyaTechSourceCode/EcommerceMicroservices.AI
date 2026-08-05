using CatalogService.Application.CreateCatalog;
using CatalogService.Application.DeleteCatalog;
using CatalogService.Application.UpdateCatalog;
using CatalogService.Infrastructure.Persistence;
using ECommerce.Contracts.Events;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Infrastructure.Messaging.Consumers
{
    public class CatalogProjectionConsumer : IConsumer<ProductCreatedEvent>,
                                             IConsumer<ProductDeletedEvent>,
                                             IConsumer<ProductUpdatedEvent>
    {
        private readonly IMediator _mediator;


        public CatalogProjectionConsumer(
            IMediator mediator,
            CatalogDbContext dbContext)
        {
            _mediator = mediator;

        }

        public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
        {
            var message = context.Message;

            try
            {

                // Send to MediatR (clean separation)
                var command = new CreateCatalogCommand
                {
                    ProductId = message.ProductId,
                    Name = message.Name,
                    AvailableStock = message.StockQuantity,
                    Description = message.Description,
                    Price = message.Price,
                    Category = message.Category,
                    Status = message.Status,
                };

                await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                // IMPORTANT: never crash consumer
                Console.WriteLine($"Error in CatalogProjectionConsumer ProductCreatedEvent: {ex.Message}");

                // In production: log + send to DLQ
                throw; // optional (MassTransit will retry if configured)
            }
        }
        public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
        {
            var message = context.Message;

            try
            {
                // Send to MediatR (clean separation)
                var command = new DeleteCatalogCommand
                {
                    ProductId = message.ProductId
                };
                await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                // IMPORTANT: never crash consumer
                Console.WriteLine($"Error in CatalogProjectionConsumer ProductDeletedEvent: {ex.Message}");

                // In production: log + send to DLQ
                throw; // optional (MassTransit will retry if configured)
            }
        }
        public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
        {
            var message = context.Message;

            try
            {
                // Send to MediatR (clean separation)
                var command = new UpdateCatalogCommand
                {
                    ProductId = message.ProductId,
                    Name = message.Name,
                    AvailableStock = message.StockQuantity,
                    Description = message.Description,
                    Price = message.Price,
                    Category = message.Category,
                    Status = message.Status,
                };
                await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                // IMPORTANT: never crash consumer
                Console.WriteLine($"Error in CatalogProjectionConsumer ProductUpdatedEvent: {ex.Message}");

                // In production: log + send to DLQ
                throw; // optional (MassTransit will retry if configured)
            }
        }
    }
}
