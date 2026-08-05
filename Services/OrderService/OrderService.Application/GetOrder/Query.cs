using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.GetOrder
{
    public record GetOrderQuery(Guid Id) : IRequest<OrderReadModel?>;
}
