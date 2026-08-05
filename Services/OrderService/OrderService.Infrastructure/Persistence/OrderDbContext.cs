using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Application.Sagas;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext, IOrderDbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderState> OrderStates { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Order aggregate
            builder.Entity<Order>()
                .OwnsMany(x => x.Items);
           
            // Saga state mapping
            builder.Entity<OrderState>(x =>
            {
                x.HasKey(p => p.CorrelationId); //  PK for saga


                x.Property(p => p.OrderId);
            });
        }
    }
}
