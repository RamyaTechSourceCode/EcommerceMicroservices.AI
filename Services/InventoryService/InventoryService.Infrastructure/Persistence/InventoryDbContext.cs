    using InventoryService.Application;
using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Infrastructure.Persistence
{
    public class InventoryDbContext : DbContext, IInventoryDbContext
    {
        public DbSet<Inventory> Inventories { get; set; }

        public DbSet<InventoryReservation> InventoryReservations { get; set; }
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Inventory>(entity =>
            {
                entity.HasKey(x => x.ProductId);

            });
        }
    }
}
