using System;
using System.Collections.Generic;
using System.Text;

namespace ProductService.Domain.Entities
{
    public class Product
    {
        public Guid Id { get;  set; }

        public string Name { get;  set; }

        public string Description { get;  set; }

        public decimal Price { get;  set; }

        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get;  set; }

        public Product(
            string name,
            string description,
            decimal price,
            string category,
            string status)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            UpdatedAt = DateTime.UtcNow;
            Category = category;
            Status = status;
        }

        public void Update(
            string name,
            string description,
            decimal price, 
            string category,
            string status)
        {
            Name = name;
            Description = description;
            Price = price;
            Category = category;
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
