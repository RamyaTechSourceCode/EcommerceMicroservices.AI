using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Orders.Kafka
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }

        public Guid CustomerId { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
