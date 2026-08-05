using ECommerce.Contracts.Events;
using ECommerce.Messaging.Abstractions;
using MediatR;
using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DeleteProduct
{
    public class DeleteProductHandler
    : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductDbContext _context;
        private readonly IEventBus _bus;

        public DeleteProductHandler(IProductDbContext context, IEventBus bus)
        {
            _context = context;
            _bus = bus;
        }

        public async Task<bool> Handle(
            DeleteProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FindAsync(request.Id);

            if (product == null)
                return false;

            product.Status = "Deleted";
            product.UpdatedAt = DateTime.UtcNow;

            
            await _context.SaveChangesAsync(cancellationToken);

            //  publish to kafka using IEventBus
            var evt = new ProductDeletedEvent
            {
                ProductId = product.Id,
            };

            await _bus.PublishAsync("product.deleted", evt);

            //publish to masstransit
            /*await _producer.Produce(new ProductCreatedEvent
            {
                ProductId = product.Id,
                Name = product.Name,
                StockQuantity = request.StockQuantity
            });*/

            return true;
        }
    }
}
