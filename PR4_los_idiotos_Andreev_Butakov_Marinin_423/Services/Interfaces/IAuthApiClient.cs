using System.Threading.Tasks;

namespace PR4_los_idiotos_Andreev_Butakov_Marinin_423.Services.Interfaces
{
    public interface IAuthApiClient
    {
        Task<bool> LoginAsync(string login, string password);
        Task<bool> RegisterAsync(string login, string password);
        void Logout();
    }
}
