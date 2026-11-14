using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces
{
    public interface IOrdersApiClient
    {
        Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(bool newestFirst);
    }
}
