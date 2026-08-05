using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application
{
 
    public class CreateInventoryCommand : IRequest
    {
        public Guid ProductId { get; init; }
        public int StockQuantity { get; set; }
    }
}
