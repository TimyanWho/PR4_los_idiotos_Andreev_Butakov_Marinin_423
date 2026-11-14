using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public class FavoritesApiClient : IFavoritesApiClient
    {
        private readonly ApiHttpClient _client;

        public FavoritesApiClient(ApiHttpClient client)
        {
            _client = client;
        }

        public async Task<List<ProductCardModel>> GetFavoritesAsync()
        {
            var items = await _client.Http.GetFromJsonAsync<List<ProductResponseDto>>("api/favorites", _client.JsonOptions);
            return items?.Select(p => p.ToCardModel()).ToList() ?? new List<ProductCardModel>();
        }

        public async Task AddToFavoritesAsync(int productId)
        {
            var response = await _client.Http.PostAsJsonAsync("api/favorites", new { ProductId = productId });
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> RemoveFavoriteAsync(int productId)
        {
            var response = await _client.Http.DeleteAsync($"api/favorites/{productId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MoveFavoriteToCartAsync(int productId)
        {
            var response = await _client.Http.PostAsync($"api/favorites/{productId}/to-cart", null);
            return response.IsSuccessStatusCode;
        }
    }
}
