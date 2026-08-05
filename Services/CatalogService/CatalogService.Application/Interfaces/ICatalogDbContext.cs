using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Interfaces
{
    public interface ICatalogDbContext
    {
        public DbSet<ProductCatalog> ProductCatalogs { get; }

        Task<int> SaveChangesAsync(
       CancellationToken cancellationToken);
    }
}
