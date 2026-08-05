using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.DeleteCatalog
{
    public class DeleteCatalogCommand : IRequest
    {
        public Guid ProductId { get; set; }
    }
}
