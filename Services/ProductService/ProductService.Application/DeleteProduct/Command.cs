using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DeleteProduct
{
    public record DeleteProductCommand(Guid Id)
    : IRequest<bool>;
}
