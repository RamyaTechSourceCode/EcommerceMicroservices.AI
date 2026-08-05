using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.DTOs
{
    public class InventoryUpdatedDto
    {
        public Guid ProductId { get; set; } 

        public int AvailableStock { get; set; }
    }
}
