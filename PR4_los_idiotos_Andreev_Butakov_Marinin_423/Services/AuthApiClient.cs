using PR4_los_idiotos_Andreev_Butakov_Marinin_423.DTO;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Models;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services;
using PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class AuthApiClient : IAuthApiClient
{
    private readonly ApiHttpClient _client;
    public AuthApiClient(ApiHttpClient client) => _client = client;

    public async Task<bool> LoginAsync(string login, string password)
    {
        var response = await _client.Http.PostAsJsonAsync("api/auth/login", new { Login = login, Password = password });
        if (!response.IsSuccessStatusCode) return false;

        var data = await response.Content.ReadFromJsonAsync<LoginResponse>(_client.JsonOptions);
        if (string.IsNullOrEmpty(data?.access_token)) return false;

        bool isAdmin = string.Equals(data.role, "Администратор", StringComparison.OrdinalIgnoreCase);
        _client.SetSession(data.access_token, login, isAdmin);
        return true;
    }

    public async Task<bool> RegisterAsync(string login, string password)
    {
        var payload = new { Login = login, Password = password, Phone = "Не указан", Address = "Не указан" };
        var response = await _client.Http.PostAsJsonAsync("api/auth/register", payload);
        return response.IsSuccessStatusCode;
    }

    public void Logout() => _client.ClearSession();
}
