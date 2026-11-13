using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public class ApiService :
        IAuthApiClient,
        ICatalogApiClient,
        ICartApiClient,
        IOrdersApiClient,
        IProductDetailsApiClient
    {
        public static ApiService Instance { get; } = new ApiService();

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private string? _accessToken;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5023/")
            };
        }

        // ==========================================
        // 1. АВТОРИЗАЦИЯ И РЕГИСТРАЦИЯ (IAuthApiClient)
        // ==========================================

        private class LoginResponse
        {
            public string? access_token { get; set; }
            public string? token_type { get; set; }
        }

        public async Task<bool> LoginAsync(string login, string password)
        {
            var payload = new { Login = login, Password = password };
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/login", payload);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var data = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            if (!string.IsNullOrEmpty(data?.access_token))
            {
                _accessToken = data.access_token;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
                return true;
            }

            return false;
        }

        public async Task<bool> RegisterAsync(string login, string password)
        {
            var payload = new
            {
                Login = login,
                Password = password,
                Phone = "Не указан",
                Address = "Не указан"
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/register", payload);
            return response.IsSuccessStatusCode;
        }

        public void Logout()
        {
            _accessToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // ==========================================
        // 2. КАТАЛОГ ТОВАРОВ (ICatalogApiClient)
        // ==========================================

        public async Task<List<Products>?> GetProductsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Products>>("api/products", _jsonOptions);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки каталога: {ex.Message}");
                return null;
            }
        }

        async Task<IReadOnlyList<ProductCardModel>> ICatalogApiClient.GetProductsAsync()
        {
            var products = await GetProductsAsync();
            if (products == null) return new List<ProductCardModel>();
            return products.Select(MapToProductCard).ToList();
        }

        public ProductCardModel MapToProductCard(Products p)
        {
            double avgRating = (p.Reviews != null && p.Reviews.Any())
                ? p.Reviews.Average(r => r.Rating)
                : 0.0;

            return new ProductCardModel
            {
                Id = p.Id,
                Name = p.Name ?? "Без названия",
                Price = p.Price,
                Description = p.Description ?? "",
                Category = p.Category?.Name ?? "Без категории",
                ImagePath = p.ImagePath ?? "",
                StockQuantity = p.StockQuantity,
                ItemsPerPackage = p.ItemsPerPackage,
                IsAvailable = p.IsAvailable && p.StockQuantity > 0,
                Rating = avgRating,
                RatingText = avgRating > 0 ? $"★ {avgRating:F1}" : "Пока нет оценок",
                AvailableText = (p.IsAvailable && p.StockQuantity > 0)
                    ? $"В наличии ({p.StockQuantity} шт.)"
                    : "Нет в наличии",
                PackageText = p.ItemsPerPackage.HasValue && p.ItemsPerPackage.Value > 1
                    ? $"В упаковке: {p.ItemsPerPackage} шт."
                    : "Обычный товар"
            };
        }

        // ==========================================
        // 3. КОРЗИНА (ICartApiClient)
        // ==========================================

        public async Task<IList<CartItemModel>> GetItemsAsync()
        {
            var items = await _httpClient.GetFromJsonAsync<List<CartItemModel>>("api/cart", _jsonOptions);
            return items ?? new List<CartItemModel>();
        }

        public async Task<bool> UpdateQuantityAsync(int productId, int quantity)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/cart/{productId}", new { Quantity = quantity });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveItemAsync(int productId)
        {
            var response = await _httpClient.DeleteAsync($"api/cart/{productId}");
            return response.IsSuccessStatusCode;
        }

        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
            await _httpClient.PostAsJsonAsync("api/cart", new { ProductId = productId, Quantity = quantity });
        }

        // ==========================================
        // 4. ИЗБРАННОЕ (для FavoritesPage)
        // ==========================================

        private class ProductResponseDto
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public decimal Price { get; set; }
            public string? Description { get; set; }
            public int CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public string? ImagePath { get; set; }
            public int StockQuantity { get; set; }
            public int? ItemsPerPackage { get; set; }
            public bool IsAvailable { get; set; }
            public double Rating { get; set; }
        }

        public async Task<List<ProductCardModel>> GetFavoritesAsync()
        {
            var items = await _httpClient.GetFromJsonAsync<List<ProductResponseDto>>("api/favorites", _jsonOptions);
            if (items == null) return new List<ProductCardModel>();

            return items.Select(p => new ProductCardModel
            {
                Id = p.Id,
                Name = p.Name ?? "Без названия",
                Price = p.Price,
                Description = p.Description ?? "",
                Category = p.CategoryName ?? "Без категории",
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
            }).ToList();
        }

        public async Task AddToFavoritesAsync(int productId)
        {
            await _httpClient.PostAsJsonAsync("api/favorites", new { ProductId = productId });
        }

        public async Task<bool> RemoveFavoriteAsync(int productId)
        {
            var response = await _httpClient.DeleteAsync($"api/favorites/{productId}");
            return response.IsSuccessStatusCode;
        }

        // Дополнительный метод под ваш эндпоинт POST api/favorites/{productId}/to-cart
        public async Task<bool> MoveFavoriteToCartAsync(int productId)
        {
            var response = await _httpClient.PostAsync($"api/favorites/{productId}/to-cart", null);
            return response.IsSuccessStatusCode;
        }

        // ==========================================
        // 5. ЗАКАЗЫ (IOrdersApiClient)
        // ==========================================

        public async Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(bool newestFirst)
        {
            var orders = await _httpClient.GetFromJsonAsync<List<OrderSummaryDto>>("api/orders", _jsonOptions);
            if (orders == null) return new List<OrderSummaryDto>();

            return newestFirst
                ? orders.OrderByDescending(o => o.OrderDate).ToList()
                : orders.OrderBy(o => o.OrderDate).ToList();
        }

        // ==========================================
        // 6. ОТЗЫВЫ (IProductDetailsApiClient)
        // ==========================================

        public async Task<IReadOnlyList<ProductReviewModel>> GetProductReviewsAsync(int productId)
        {
            var reviews = await _httpClient.GetFromJsonAsync<List<ProductReviewModel>>($"api/products/{productId}/reviews", _jsonOptions);
            return reviews ?? new List<ProductReviewModel>();
        }

        public async Task SubmitReviewAsync(int productId, int rating, string text)
        {
            await _httpClient.PostAsJsonAsync($"api/products/{productId}/reviews", new { Rating = rating, Text = text });
        }
    }
}
