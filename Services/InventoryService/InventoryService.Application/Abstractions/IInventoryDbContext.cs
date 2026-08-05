using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application
{
    public interface IInventoryDbContext
    {
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryReservation> InventoryReservations { get; set; }
        Task<int> SaveChangesAsync(
       CancellationToken cancellationToken);
    }
}
