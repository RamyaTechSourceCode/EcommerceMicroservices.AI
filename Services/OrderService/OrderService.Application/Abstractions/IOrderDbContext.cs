
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Sagas;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application
{
    public interface IOrderDbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderState> OrderStates { get; set; }
        Task<int> SaveChangesAsync(
       CancellationToken cancellationToken);
    }
}
