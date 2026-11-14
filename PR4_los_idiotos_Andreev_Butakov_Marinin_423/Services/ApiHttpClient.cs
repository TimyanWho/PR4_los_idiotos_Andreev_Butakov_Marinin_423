using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public class ApiHttpClient
    {
        public HttpClient Http { get; }
        public JsonSerializerOptions JsonOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public string? AccessToken { get; private set; }
        public string? CurrentUserLogin { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

        public ApiHttpClient()
        {
            Http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5023/")
            };
        }

        public void SetSession(string token, string login)
        {
            AccessToken = token;
            CurrentUserLogin = login;
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearSession()
        {
            AccessToken = null;
            CurrentUserLogin = null;
            Http.DefaultRequestHeaders.Authorization = null;
        }
    }
}
