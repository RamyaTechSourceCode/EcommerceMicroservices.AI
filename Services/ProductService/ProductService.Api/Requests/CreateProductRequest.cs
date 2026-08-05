namespace ProductService.Api.Requests
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } 
        public string Status { get; set; } 
        public DateTime UpdatedAt { get; private set; }
    }
}
