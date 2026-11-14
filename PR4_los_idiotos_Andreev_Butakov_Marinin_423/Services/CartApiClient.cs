using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public class CartApiClient : ICartApiClient
    {
        private readonly ApiHttpClient _client;

        public CartApiClient(ApiHttpClient client)
        {
            _client = client;
        }

        public async Task<IList<CartItemModel>> GetItemsAsync()
        {
            var response = await _client.Http.GetFromJsonAsync<CartResponseDto>("api/cart", _client.JsonOptions);
            if (response?.Items == null)
            {
                return new List<CartItemModel>();
            }

            return response.Items
                .Where(i => i.Product != null)
                .Select(i => new CartItemModel
                {
                    ProductId = i.Product!.Id,
                    Name = i.Product.Name ?? "Без названия",
                    Description = i.Product.Description ?? "",
                    UnitPrice = i.Product.Price,
                    StockQuantity = i.Product.StockQuantity,
                    Quantity = i.Quantity
                })
                .ToList();
        }

        public async Task<bool> UpdateQuantityAsync(int productId, int quantity)
        {
            var response = await _client.Http.PutAsJsonAsync($"api/cart/{productId}", new { Quantity = quantity });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveItemAsync(int productId)
        {
            var response = await _client.Http.DeleteAsync($"api/cart/{productId}");
            return response.IsSuccessStatusCode;
        }

        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
            var response = await _client.Http.PostAsJsonAsync("api/cart", new { ProductId = productId, Quantity = quantity });
            response.EnsureSuccessStatusCode();
        }
    }
}
