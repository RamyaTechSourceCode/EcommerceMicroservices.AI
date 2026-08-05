using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Domain.Entities
{
    public class InventoryReservation
    {
        public Guid Id { get; set; }

        public Guid CorrelationId { get; set; }   // Saga ID
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime ProcessedAt { get; set; }
    }
}
