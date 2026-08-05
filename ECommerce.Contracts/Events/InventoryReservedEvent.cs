using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Contracts.Events
{
    public record InventoryReservedEvent
    {
        public Guid CorrelationId { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; init; }
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    }
}
