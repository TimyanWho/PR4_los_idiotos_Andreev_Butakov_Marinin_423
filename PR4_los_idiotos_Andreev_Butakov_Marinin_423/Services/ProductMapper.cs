using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public static class ProductMapper
    {
        public static ProductCardModel ToCardModel(this ProductResponseDto p)
        {
            return new ProductCardModel
            {
                Id = p.Id,
                Name = p.Name ?? "Без названия",
                Price = p.Price,
                Description = p.Description ?? "",
                Category = string.IsNullOrWhiteSpace(p.CategoryName) ? "Без категории" : p.CategoryName,
                ImagePath = p.ImagePath ?? "",
                StockQuantity = p.StockQuantity,
                ItemsPerPackage = p.ItemsPerPackage,
                IsAvailable = p.IsAvailable && p.StockQuantity > 0,
                Rating = p.Rating,
                RatingText = p.Rating > 0 ? $"★ {p.Rating:F1}" : "Пока нет оценок",
                AvailableText = (p.IsAvailable && p.StockQuantity > 0)
                    ? $"В наличии ({p.StockQuantity} шт.)"
                    : "Нет в наличии",
                PackageText = p.ItemsPerPackage.HasValue && p.ItemsPerPackage.Value > 1
                    ? $"В упаковке: {p.ItemsPerPackage} шт."
                    : "Обычный товар"
            };
        }
    }
}
