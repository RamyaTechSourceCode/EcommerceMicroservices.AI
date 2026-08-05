using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Domain.Entities
{
    public class ProductCatalog
    {
        public Guid ProductId { get; set; } 

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int AvailableStock { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}
