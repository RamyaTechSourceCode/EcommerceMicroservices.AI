using CatalogService.Application.DTOs;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("catalog")]
    public class CatalogSyncController : ControllerBase
    {
        private readonly CatalogDbContext _db;

        public CatalogSyncController(CatalogDbContext db)
        {
            _db = db;
        }

        [HttpPost("product-updated")]
        public async Task<IActionResult> ProductUpdated(
            [FromBody] ProductUpdatedDto dto)
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

            item.Name = dto.Name;
            item.Category = dto.Category;
            item.Price = dto.Price;
            item.Status = dto.Status;
            item.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
