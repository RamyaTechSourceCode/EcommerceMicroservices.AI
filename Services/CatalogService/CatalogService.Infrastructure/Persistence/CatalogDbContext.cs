using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Infrastructure.Persistence
{
    public class CatalogDbContext : DbContext, ICatalogDbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProductCatalog> ProductCatalogs => Set<ProductCatalog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProductCatalog>()
                .HasKey(x => x.ProductId);
        }
    }
}
