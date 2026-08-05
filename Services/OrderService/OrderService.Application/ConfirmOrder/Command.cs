using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application
{
    public record ConfirmOrderCommand(Guid CorrelationId,Guid OrderId) : IRequest;
}
