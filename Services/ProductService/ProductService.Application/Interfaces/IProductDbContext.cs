using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IProductDbContext
    {
        public DbSet<Product> Products { get; }

        Task<int> SaveChangesAsync(
       CancellationToken cancellationToken);
    }
}
