using System;

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
        
        // NEW: Unit ID
        // Even if DB is 'unit_id', we usually use PascalCase in C#
        public int UnitId { get; set; } 
        public string? UnitName { get; set; } // For display

        public string? ImagePath { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime InsertedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    // You also need a Unit model for the dropdown list
    public class Unit
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
    }
}