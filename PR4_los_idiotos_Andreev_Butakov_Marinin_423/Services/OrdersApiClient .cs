using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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
            if (orders == null) return new List<OrderSummaryDto>();

            return newestFirst
                ? orders.OrderByDescending(o => o.OrderDate).ToList()
                : orders.OrderBy(o => o.OrderDate).ToList();
        }

        public async Task<IReadOnlyList<PickupPointOptionDto>> GetPickupPointsAsync()
        {
            var points = await _client.Http.GetFromJsonAsync<List<PickupPointOptionDto>>("api/pickup-points", _client.JsonOptions);
            return points ?? new List<PickupPointOptionDto>();
        }

        public async Task<OrderCreatedResultDto> CreateDirectOrderAsync(
            int productId,
            int quantity,
            string deliveryType,
            string? deliveryAddress,
            int? pickupPointId)
        {
            var payload = new
            {
                ProductId = productId,
                Quantity = quantity,
                DeliveryType = deliveryType,
                DeliveryAddress = deliveryAddress,
                PickupPointId = pickupPointId
            };

            var response = await _client.Http.PostAsJsonAsync("api/orders/direct", payload);
            return await HandleOrderResponseAsync(response);
        }

        public async Task<OrderCreatedResultDto> CreateOrderFromCartAsync(
            string deliveryType,
            string? deliveryAddress,
            int? pickupPointId)
        {
            var payload = new
            {
                DeliveryType = deliveryType,
                DeliveryAddress = deliveryAddress,
                PickupPointId = pickupPointId
            };

            var response = await _client.Http.PostAsJsonAsync("api/orders/from-cart", payload);
            return await HandleOrderResponseAsync(response);
        }

        private async Task<OrderCreatedResultDto> HandleOrderResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(errorJson);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                    {
                        throw new Exception(msg.GetString());
                    }
                }
                catch (JsonException) { }

                throw new Exception($"Ошибка сервера: {(int)response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<OrderCreatedResultDto>(_client.JsonOptions);
            return result ?? throw new Exception("Пустой ответ от сервера.");
        }

        public async Task<List<PickupPointOptionDto>> GetAdminPickupPointsAsync()
        {
            return await _client.Http.GetFromJsonAsync<List<PickupPointOptionDto>>("api/pickup-points", _client.JsonOptions)
                   ?? new List<PickupPointOptionDto>();
        }

        public async Task CreatePickupPointAsync(string name, string address)
        {
            var res = await _client.Http.PostAsJsonAsync("api/pickup-points", new { Name = name, Address = address });
            await _client.EnsureSuccessAsync(res);
        }

        public async Task UpdatePickupPointAsync(int id, string name, string address)
        {
            var res = await _client.Http.PutAsJsonAsync($"api/pickup-points/{id}", new { Name = name, Address = address });
            await _client.EnsureSuccessAsync(res);
        }

        public async Task DeletePickupPointAsync(int id)
        {
            var res = await _client.Http.DeleteAsync($"api/pickup-points/{id}");
            await _client.EnsureSuccessAsync(res);
        }

        public async Task<List<OrderStatusOption>> GetOrderStatusesAsync()
        {
            return await _client.Http.GetFromJsonAsync<List<OrderStatusOption>>("api/orders/statuses", _client.JsonOptions)
                   ?? new List<OrderStatusOption>();
        }

        public async Task<List<AdminOrderModel>> GetAdminOrdersAsync()
        {
            return await _client.Http.GetFromJsonAsync<List<AdminOrderModel>>("api/orders/all", _client.JsonOptions)
                   ?? new List<AdminOrderModel>();
        }

        public async Task UpdateOrderStatusAsync(int orderId, int statusId)
        {
            var res = await _client.Http.PutAsJsonAsync($"api/orders/{orderId}/status", new { StatusId = statusId });
            await _client.EnsureSuccessAsync(res);
        }
    }
}
