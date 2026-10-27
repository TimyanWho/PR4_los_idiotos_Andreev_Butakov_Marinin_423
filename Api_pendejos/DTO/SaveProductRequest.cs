using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_pendejos.DTO
{
    public record SaveProductRequest(
        string Name,
        decimal Price,
        string Description,
        int CategoryId,
        string ImagePath,
        int StockQuantity,
        int? ItemsPerPackage,
        bool IsAvailable
    );
}
