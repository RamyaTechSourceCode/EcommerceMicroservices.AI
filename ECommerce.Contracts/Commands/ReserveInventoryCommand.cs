using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Contracts.Commands
{
 
    public class ReserveInventoryCommand : IRequest
    {
        public Guid CorrelationId { get; set; }
        public Guid OrderId { get; init; }
        public Guid ProductId { get; init; }
        public int StockQuantity { get; set; }
    }
}
