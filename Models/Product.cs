namespace trial.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } 

        public string? ImagePath { get; set; }
    }
}