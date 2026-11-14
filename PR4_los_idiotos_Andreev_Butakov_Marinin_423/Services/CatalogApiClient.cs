using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Linq;

public class CatalogApiClient : ICatalogApiClient
{
    private readonly ApiHttpClient _client;
    public CatalogApiClient(ApiHttpClient client) => _client = client;

    public async Task<IReadOnlyList<ProductCardModel>> GetProductsAsync()
    {
        var items = await _client.Http.GetFromJsonAsync<List<ProductResponseDto>>("api/products", _client.JsonOptions);
        return items?.Select(p => p.ToCardModel()).ToList() ?? new List<ProductCardModel>();
    }

    public async Task<IReadOnlyList<Categories>> GetCategoriesAsync()
    {
        return await _client.Http.GetFromJsonAsync<List<Categories>>("api/categories", _client.JsonOptions)
               ?? new List<Categories>();
    }
}