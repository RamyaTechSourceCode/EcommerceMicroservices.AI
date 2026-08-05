using CatalogService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("api/catalogs")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogDbContext _db;

        public CatalogController(CatalogDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _db.ProductCatalogs
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Ok(products);
        }


        [HttpGet("paged")]
        public async Task<IActionResult> GetPaginatedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            var totalRecords = await _db.ProductCatalogs.CountAsync();

            var inventory = await _db.ProductCatalogs
                .OrderBy(i => i.UpdatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                Data = inventory
            });
        }
    }
}