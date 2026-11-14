using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

public class ApiHttpClient
{
    public HttpClient Http { get; }
    public JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string? AccessToken { get; private set; }
    public string? CurrentUserLogin { get; private set; }
    public bool IsAdministrator { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public ApiHttpClient()
    {
        Http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5023/")
        };
    }

    public void SetSession(string token, string login, bool isAdministrator)
    {
        AccessToken = token;
        CurrentUserLogin = login;
        IsAdministrator = isAdministrator;
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearSession()
    {
        AccessToken = null;
        CurrentUserLogin = null;
        IsAdministrator = false;
        Http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    throw new Exception(msg.GetString());
            }
            catch (JsonException) { }

            throw new Exception($"Ошибка сервера: {(int)response.StatusCode}");
        }
    }
}
