using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            HttpClient _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5023/")
            };
        }

        public async Task<users?> GetUserAsync(int id)
        {
            try
            {
                users? user = await _httpClient.GetFromJsonAsync<users>($"api/users/{id}");
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
