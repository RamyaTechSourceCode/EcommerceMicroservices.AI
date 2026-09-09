using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CatalogService.Application.GetCatalogById
{
    public class GetCatalogById
    : IRequestHandler<GetCatalogByIdQuery, ProductCatalog?>
    {
        private readonly ICatalogDbContext _context;

        public GetCatalogById(ICatalogDbContext context)
        {
            _context = context;
        }

        public async Task<ProductCatalog?> Handle(
            GetCatalogByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.ProductCatalogs
            .Where(x => x.ProductId == request.Id)
            .Select(x => new ProductCatalog
            {
                ProductId = x.ProductId,
                Name = x.Name,
                Price = x.Price,
                Description = x.Description
            })
            .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
