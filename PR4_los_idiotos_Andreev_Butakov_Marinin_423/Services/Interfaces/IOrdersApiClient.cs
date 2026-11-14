using System.Collections.Generic;
using System.Threading.Tasks;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces
{
    public interface IOrdersApiClient
    {
        Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(bool newestFirst);
        Task<IReadOnlyList<PickupPointOptionDto>> GetPickupPointsAsync();
        Task<OrderCreatedResultDto> CreateDirectOrderAsync(int productId, int quantity, string deliveryType, string? deliveryAddress, int? pickupPointId);
        Task<OrderCreatedResultDto> CreateOrderFromCartAsync(string deliveryType, string? deliveryAddress, int? pickupPointId);

        Task<List<OrderStatusOption>> GetOrderStatusesAsync();
        Task<List<AdminOrderModel>> GetAdminOrdersAsync();
        Task UpdateOrderStatusAsync(int orderId, int statusId);
        Task<List<PickupPointOptionDto>> GetAdminPickupPointsAsync();
        Task CreatePickupPointAsync(string name, string address);
        Task UpdatePickupPointAsync(int id, string name, string address);
        Task DeletePickupPointAsync(int id);
    }
}
