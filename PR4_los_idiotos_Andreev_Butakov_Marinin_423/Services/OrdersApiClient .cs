using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public class OrdersApiClient : IOrdersApiClient
    {
        private readonly ApiHttpClient _client;

        public OrdersApiClient(ApiHttpClient client)
        {
            _client = client;
        }

        public async Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(bool newestFirst)
        {
            var orders = await _client.Http.GetFromJsonAsync<List<OrderSummaryDto>>("api/orders", _client.JsonOptions);
            if (orders == null)
            {
                return new List<OrderSummaryDto>();
            }

            return newestFirst
                ? orders.OrderByDescending(o => o.OrderDate).ToList()
                : orders.OrderBy(o => o.OrderDate).ToList();
        }
    }
}