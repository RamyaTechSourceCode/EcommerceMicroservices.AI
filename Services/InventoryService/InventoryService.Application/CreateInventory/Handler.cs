using InventoryService.Application.Abstractions;
using InventoryService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application
{
   

    public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand>
    {
        private readonly IInventoryDbContext _inventoryDbContext;
        private readonly IRedisService _redisService;
        public CreateInventoryCommandHandler(IInventoryDbContext inventoryDbContext,
        IRedisService redisService)
        {
            _inventoryDbContext = inventoryDbContext;
            _redisService = redisService;
        }

        public async Task Handle(
            CreateInventoryCommand request,
            CancellationToken cancellationToken)
        {
            // Idempotency check
            // Prevent duplicate inventory records
            var exists = await _inventoryDbContext.Inventories
                .AnyAsync(
                    x => x.ProductId == request.ProductId,
                    cancellationToken);

            if (exists)
                return;

            var inventory = new Inventory
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Quantity =request.StockQuantity
            };

            _inventoryDbContext.Inventories.Add(inventory);

            await _inventoryDbContext.SaveChangesAsync(cancellationToken);

            // Initialize Redis stock cache
            await _redisService.SetStock(
                request.ProductId.ToString(),
                0);

        }
    }
}
