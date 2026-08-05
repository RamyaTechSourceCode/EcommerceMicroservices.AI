using CatalogService.Application.DTOs;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("catalog")]
    public class InventorySyncController : ControllerBase
    {
        private readonly CatalogDbContext _db;

        public InventorySyncController(CatalogDbContext db)
        {
            _db = db;
        }

        [HttpPost("inventory-updated")]
        public async Task<IActionResult> InventoryUpdated(
            [FromBody] InventoryUpdatedDto dto)
        {
            var item = await _db.ProductCatalogs
                .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);

            if (item == null)
            {
                item = new ProductCatalog
                {
                    ProductId = dto.ProductId
                };

                _db.ProductCatalogs.Add(item);
            }

            item.AvailableStock = dto.AvailableStock;
            item.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
