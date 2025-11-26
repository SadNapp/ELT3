using System;
using System.ComponentModel.DataAnnotations;

namespace ELT3.Models
{
    public class StockQuote
    {
        [Key]
        public int Id { get; set; }
        
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal ChangesPercentage { get; set; }
        
        
        public DateTime LastUpdateTime  { get; set; }
        public DateTime RecordedAt { get; set; } 
    }
}