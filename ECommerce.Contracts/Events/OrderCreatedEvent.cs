using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Contracts.Events
{
    public record OrderCreatedEvent 
    {
        public Guid CorrelationId { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
        public List<OrderItemEvent> Items { get; set; } = new();
    }
}
