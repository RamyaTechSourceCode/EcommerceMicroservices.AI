using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Contracts.Events
{
    public record ProductCreatedEvent
    {
        public Guid ProductId { get; init; }
        public string Name { get; init; }

        public int StockQuantity { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
    }
}
