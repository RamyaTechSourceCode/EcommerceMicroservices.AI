using MediatR;
using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.GetProducts
{
    public record GetProductsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null
    ) : IRequest<List<ProductDto>>;
}
