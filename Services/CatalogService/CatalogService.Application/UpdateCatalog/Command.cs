using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.UpdateCatalog
{
   public class UpdateCatalogCommand : IRequest
    {
        [Required]
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Category { get; set; }
        public decimal Price { get; set; }
        public int AvailableStock { get; set; }
        public string Status { get; set; }
    }
}
