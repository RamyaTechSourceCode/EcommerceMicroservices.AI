using CatalogService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.GetCatalogById
{
    public record GetCatalogByIdQuery(Guid Id) : IRequest<ProductCatalog>;
}
