using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.GetOrder
{
    public class OrderReadModel
    {
        public Guid OrderId { get; set; }

        public Guid CustomerId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
    }
}
