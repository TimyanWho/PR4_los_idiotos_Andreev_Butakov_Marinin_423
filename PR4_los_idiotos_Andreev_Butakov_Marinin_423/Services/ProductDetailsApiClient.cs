using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public class ProductDetailsApiClient : IProductDetailsApiClient
    {
        private readonly ApiHttpClient _client;

        public ProductDetailsApiClient(ApiHttpClient client)
        {
            _client = client;
        }

        public async Task<IReadOnlyList<ProductReviewModel>> GetProductReviewsAsync(int productId)
        {
            var response = await _client.Http.GetFromJsonAsync<ReviewsResponseDto>($"api/reviews?productId={productId}", _client.JsonOptions);
            if (response?.Reviews == null)
            {
                return new List<ProductReviewModel>();
            }

            return response.Reviews.Select(r => new ProductReviewModel
            {
                AuthorName = string.IsNullOrWhiteSpace(r.Author) ? "Пользователь" : r.Author,
                Rating = r.Rating,
                Text = r.Comment ?? "",
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminReviewModel>> GetAdminReviewsAsync(int productId)
        {
            var res = await _client.Http.GetFromJsonAsync<ReviewsResponseDto>($"api/reviews?productId={productId}", _client.JsonOptions);
            if (res?.Reviews == null) return new List<AdminReviewModel>();

            return res.Reviews.Select(r => new AdminReviewModel
            {
                Id = r.Id,
                Author = r.Author ?? "Пользователь",
                Rating = r.Rating,
                Comment = r.Comment ?? "",
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            var res = await _client.Http.DeleteAsync($"api/reviews/{reviewId}");
            await _client.EnsureSuccessAsync(res);
        }

        public async Task SubmitReviewAsync(int productId, int rating, string text)
        {
            var payload = new
            {
                ProductId = productId,
                Rating = rating,
                Comment = text
            };

            var response = await _client.Http.PostAsJsonAsync("api/reviews", payload);
            response.EnsureSuccessStatusCode();
        }
    }
}
