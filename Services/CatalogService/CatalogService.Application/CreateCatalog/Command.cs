using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.CreateCatalog
{
    public class CreateCatalogCommand : IRequest
    {
        public Guid ProductId { get; set; } 
        public string Name { get; set; } 
        public string Description { get; set; } 

        public string Category { get; set; }
        public decimal Price { get; set; }
        public int AvailableStock { get; set; }
        public string Status { get; set; } 
    }
}
