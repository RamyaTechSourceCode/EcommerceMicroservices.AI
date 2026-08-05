using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Contracts.Events
{
    public record ProductDeletedEvent
    {
        public Guid ProductId { get; init; }
       
    }
}
