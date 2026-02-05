using System.ComponentModel.DataAnnotations;

namespace trial.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        
 
        public required string ProductName { get; set; }
        
      
        public required decimal Price { get; set; }
        
        public required int StockQuantity { get; set; }
        
    
        public  required int CategoryId { get; set; }
        
        public string? CategoryName { get; set; } 
    }
}