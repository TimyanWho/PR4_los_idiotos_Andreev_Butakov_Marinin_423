using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ImagePath { get; set; }
        public int StockQuantity { get; set; }
        public int? ItemsPerPackage { get; set; }
        public bool IsAvailable { get; set; }
        public double Rating { get; set; }
    }
}
