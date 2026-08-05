using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Contracts.Events
{
    public record ReserveInventory(
    Guid CorrelationId,
    Guid OrderId,
    Guid ProductId,
    int Quantity);
}
