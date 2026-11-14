using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models
{
    public class AdminProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Description { get; set; } = "";
        public int CategoryId { get; set; }
        public string Category { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public int StockQuantity { get; set; }
        public int? ItemsPerPackage { get; set; }
        public bool IsAvailable { get; set; }
        public double Rating { get; set; }
    }

    public class AdminOrderModel
    {
        public int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Buyer { get; set; } = "";
        public string Status { get; set; } = "";
        public string DeliveryType { get; set; } = "";
        public decimal TotalPrice { get; set; }
    }

    public class OrderStatusOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public override string ToString()
        {
            return Name;
        }
    }

    public class AdminReviewModel
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
        public string Author { get; set; } = "";
    }
}
