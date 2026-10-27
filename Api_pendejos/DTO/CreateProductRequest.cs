using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_pendejos.DTO
{
    public record ProductResponse(
        int Id,
        string Name,
        decimal Price,
        string Description,
        int CategoryId,
        string CategoryName,
        string ImagePath,
        int StockQuantity,
        int? ItemsPerPackage,
        bool IsAvailable,
        double AverageRating
    );
}
