using System.Collections.Generic;
using System.Threading.Tasks;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    /// <summary>
    /// WPF-side operations used by the product details page.
    /// Implement the HTTP adapter after the API routes and response DTOs are agreed.
    /// Review loading must be public; user actions require an authenticated session.
    /// </summary>
    public interface IProductDetailsApiClient
    {
        bool IsAuthenticated { get; }

        Task<IReadOnlyList<ProductReviewModel>> GetProductReviewsAsync(int productId);
        Task AddToCartAsync(int productId, int quantity);
        Task AddToFavoritesAsync(int productId);
        Task SubmitReviewAsync(int productId, int rating, string text);
    }
}