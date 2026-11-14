using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Pages;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public class ApiService
    {
        public static ApiService Instance { get; } = new ApiService();

        private readonly ApiHttpClient _httpClient = new();

        public bool IsAuthenticated => _httpClient.IsAuthenticated;
        public string? CurrentUserLogin => _httpClient.CurrentUserLogin;
        public bool IsAdministrator => _httpClient.IsAdministrator;

        public IAuthApiClient Auth { get; }
        public ICatalogApiClient Catalog { get; }
        public ICartApiClient Cart { get; }
        public IFavoritesApiClient Favorites { get; }
        public IOrdersApiClient Orders { get; }
        public IProductDetailsApiClient ProductDetails { get; }

        private ApiService()
        {
            Auth = new AuthApiClient(_httpClient);
            Catalog = new CatalogApiClient(_httpClient);
            Cart = new CartApiClient(_httpClient);
            Favorites = new FavoritesApiClient(_httpClient);
            Orders = new OrdersApiClient(_httpClient);
            ProductDetails = new ProductDetailsApiClient(_httpClient);
        }
    }
}
