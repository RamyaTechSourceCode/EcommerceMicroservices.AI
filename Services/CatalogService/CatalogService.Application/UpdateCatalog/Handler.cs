using CatalogService.Application.CreateCatalog;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.UpdateCatalog
{
    public class UpdateCatalogCommandHandler : IRequestHandler<UpdateCatalogCommand>
    {
        private readonly ICatalogDbContext _catalogDbContext;

        public UpdateCatalogCommandHandler(ICatalogDbContext catalogDbContext)
        {
            _catalogDbContext = catalogDbContext;

        }

        public async Task Handle(
            UpdateCatalogCommand request,
            CancellationToken cancellationToken)
        {

            // Idempotency check
            // Prevent duplicate inventory records
            var catalog = await _catalogDbContext.ProductCatalogs
                .FindAsync(request.ProductId,
                    cancellationToken);

            if (catalog == null) return;

            catalog.Name = request.Name;
            catalog.Description = request.Description;
            catalog.Price = request.Price;
            catalog.AvailableStock = request.AvailableStock;
            catalog.Status = request.Status;
            catalog.Category = request.Category;
            catalog.UpdatedAt = DateTime.UtcNow;

            _catalogDbContext.ProductCatalogs.Update(catalog);

            await _catalogDbContext.SaveChangesAsync(cancellationToken);


        }
    }
}
