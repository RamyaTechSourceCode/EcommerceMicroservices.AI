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

namespace CatalogService.Application.DeleteCatalog
{
    public class DeleteCatalogCommandHandler : IRequestHandler<DeleteCatalogCommand>
    {
        private readonly ICatalogDbContext _catalogDbContext;

        public DeleteCatalogCommandHandler(ICatalogDbContext catalogDbContext)
        {
            _catalogDbContext = catalogDbContext;

        }

        public async Task Handle(
            DeleteCatalogCommand request,
            CancellationToken cancellationToken)
        {

            // check if record exists, if not return
            var catalog = await _catalogDbContext.ProductCatalogs
                .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);


            if (catalog == null) return;


            _catalogDbContext.ProductCatalogs.Remove(catalog);

            await _catalogDbContext.SaveChangesAsync(cancellationToken);

        }
    }
}
