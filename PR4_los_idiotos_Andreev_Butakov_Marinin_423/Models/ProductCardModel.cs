namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models
{
    public class ProductCardModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public int StockQuantity { get; set; }
        public int? ItemsPerPackage { get; set; }
        public bool IsAvailable { get; set; }
        public double Rating { get; set; }
        public string RatingText { get; set; } = "";
        public string AvailableText { get; set; } = "";
        public string PackageText { get; set; } = "";
    }
}