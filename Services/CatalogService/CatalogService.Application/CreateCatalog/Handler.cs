using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.CreateCatalog
{
    public class CreateCatalogCommandHandler : IRequestHandler<CreateCatalogCommand>
    {
        private readonly ICatalogDbContext _catalogDbContext;
       
        public CreateCatalogCommandHandler(ICatalogDbContext catalogDbContext)
        {
            _catalogDbContext = catalogDbContext;
          
        }

        public async Task Handle(
            CreateCatalogCommand request,
            CancellationToken cancellationToken)
        {
            // Idempotency check
            // Prevent duplicate inventory records
            var exists = await _catalogDbContext.ProductCatalogs
                .AnyAsync(
                    x => x.ProductId == request.ProductId,
                    cancellationToken);

            if (exists)
                return;

            var catalog = new ProductCatalog
            {
                ProductId = request.ProductId,
                Price = request.Price,
                Description = request.Description,
                Name = request.Name,
                AvailableStock = request.AvailableStock,
                Category = request.Category,
                Status = request.Status,
                UpdatedAt = DateTime.UtcNow,
            };

            _catalogDbContext.ProductCatalogs.Add(catalog);

            await _catalogDbContext.SaveChangesAsync(cancellationToken);

         
        }
    }
}
