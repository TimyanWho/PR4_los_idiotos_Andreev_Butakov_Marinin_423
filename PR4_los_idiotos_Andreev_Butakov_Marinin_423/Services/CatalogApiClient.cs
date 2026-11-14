using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class CatalogApiClient : ICatalogApiClient
{
    private readonly ApiHttpClient _client;
    public CatalogApiClient(ApiHttpClient client) => _client = client;

    public async Task<IReadOnlyList<ProductCardModel>> GetProductsAsync()
    {
        var items = await _client.Http.GetFromJsonAsync<List<ProductResponseDto>>("api/products", _client.JsonOptions);
        return items?.Select(p => p.ToCardModel()).ToList() ?? new List<ProductCardModel>();
    }

    public async Task<List<AdminProductModel>> GetAdminProductsAsync()
    {
        var items = await _client.Http.GetFromJsonAsync<List<ProductResponseDto>>("api/products", _client.JsonOptions);
        if (items == null) return new List<AdminProductModel>();

        return items.Select(p => new AdminProductModel
        {
            Id = p.Id,
            Name = p.Name ?? "",
            Price = p.Price,
            Description = p.Description ?? "",
            CategoryId = p.CategoryId,
            Category = p.CategoryName ?? "",
            ImagePath = p.ImagePath ?? "",
            StockQuantity = p.StockQuantity,
            ItemsPerPackage = p.ItemsPerPackage,
            IsAvailable = p.IsAvailable
        }).ToList();
    }

    public async Task CreateProductAsync(AdminProductModel model)
    {
        var res = await _client.Http.PostAsJsonAsync("api/products", model);
        await _client.EnsureSuccessAsync(res);
    }

    public async Task UpdateProductAsync(int id, AdminProductModel model)
    {
        var res = await _client.Http.PutAsJsonAsync($"api/products/{id}", model);
        await _client.EnsureSuccessAsync(res);
    }

    //categories

    public async Task<IReadOnlyList<Categories>> GetCategoriesAsync()
    {
        return await _client.Http.GetFromJsonAsync<List<Categories>>("api/categories", _client.JsonOptions)
               ?? new List<Categories>();
    }

    public async Task CreateCategoryAsync(string name)
    {
        var res = await _client.Http.PostAsJsonAsync("api/categories", new { Name = name });
        await _client.EnsureSuccessAsync(res);
    }

    public async Task UpdateCategoryAsync(int id, string name)
    {
        var res = await _client.Http.PutAsJsonAsync($"api/categories/{id}", new { Name = name });
        await _client.EnsureSuccessAsync(res);
    }
}