using System.Collections.Generic;
using System.Threading.Tasks;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces
{
    public interface ICatalogApiClient
    {
        Task<IReadOnlyList<ProductCardModel>> GetProductsAsync();
        Task<IReadOnlyList<Categories>> GetCategoriesAsync();
    }
}