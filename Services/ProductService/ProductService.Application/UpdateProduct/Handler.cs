using ECommerce.Contracts.Events;
using ECommerce.Messaging.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.UpdateProducts
{
    public class UpdateProductHandler
      : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductDbContext _context;
        private readonly IEventBus _bus;
        public UpdateProductHandler(IProductDbContext context, IEventBus bus)
        {
            _context = context;
            _bus = bus;
        }

        public async Task<bool> Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

            if (product is null)
                return false;

            product.Update(
                request.Name,
                request.Description,
                request.Price,
                request.Category,
                request.Status);

            await _context.SaveChangesAsync(cancellationToken);
            //  publish to kafka using IEventBus
            var evt = new ProductUpdatedEvent
            {
                ProductId = product.Id,
                Name = request.Name,
                StockQuantity = request.StockQuantity,
                Price = request.Price,
                Description = request.Description,
                Category = request.Category,
                Status = request.Status
            };
            await _bus.PublishAsync("product.updated", evt);

            return true;
        }
    }
}
