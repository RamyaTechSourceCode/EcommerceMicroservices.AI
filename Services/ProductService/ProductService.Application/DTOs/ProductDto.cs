using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
