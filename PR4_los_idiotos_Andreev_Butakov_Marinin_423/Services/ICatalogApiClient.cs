using System.Collections.Generic;
using System.Threading.Tasks;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    /// <summary>
    /// WPF-side contract for loading the public product catalog from the API.
    /// The concrete HTTP client is added after the API routes and response DTOs
    /// are agreed with the API owner.
    /// </summary>
    public interface ICatalogApiClient
    {
        Task<IReadOnlyList<ProductCardModel>> GetProductsAsync();
    }
}
