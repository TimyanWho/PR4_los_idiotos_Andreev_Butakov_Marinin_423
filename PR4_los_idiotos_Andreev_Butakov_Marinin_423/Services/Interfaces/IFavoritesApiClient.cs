using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces
{
    public interface IFavoritesApiClient
    {
        Task<List<ProductCardModel>> GetFavoritesAsync();
        Task AddToFavoritesAsync(int productId);
        Task<bool> RemoveFavoriteAsync(int productId);
        Task<bool> MoveFavoriteToCartAsync(int productId);
    }
}
