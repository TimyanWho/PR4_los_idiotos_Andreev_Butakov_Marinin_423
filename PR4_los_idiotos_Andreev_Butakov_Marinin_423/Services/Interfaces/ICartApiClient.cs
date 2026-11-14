using System.Collections.Generic;
using System.Threading.Tasks;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces
{
    public interface ICartApiClient
    {
        Task<IList<CartItemModel>> GetItemsAsync();
        Task<bool> UpdateQuantityAsync(int productId, int quantity);
        Task<bool> RemoveItemAsync(int productId);
        Task AddToCartAsync(int productId, int quantity = 1);
    }
}
