using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.CreateOrder
{
    public record CreateOrderCommand(
     Guid CustomerId,
     List<CreateOrderItemDto> Items
 ) : IRequest<Guid>;

    public record CreateOrderItemDto(
        Guid ProductId,
        int Quantity,
        decimal Price
    );
}
