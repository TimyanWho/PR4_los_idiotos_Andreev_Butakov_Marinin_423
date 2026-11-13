namespace Api_pendejos.DTO
{
    public record LoginRequest(string Login, string Password);
    public record RegisterRequest(
    string Login,
    string Password);
}
