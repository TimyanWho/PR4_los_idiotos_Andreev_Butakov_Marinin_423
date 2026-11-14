using System.Collections.Generic;
using System.Threading.Tasks;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public interface ICatalogApiClient
    {
        Task<IReadOnlyList<ProductCardModel>> GetProductsAsync();
        Task<IReadOnlyList<Categories>> GetCategoriesAsync();
    }
}
