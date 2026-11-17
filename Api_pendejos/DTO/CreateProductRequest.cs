using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api_pendejos.DTO
{
    public record CreateProductRequest(
        string Name,
        decimal Price
    );
}
