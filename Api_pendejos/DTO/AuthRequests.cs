namespace Api_pendejos.DTO
{
    public record LoginRequest(string Login, string Password);
    public record RegisterRequest(
    string Phone,
    string Address,
    string Login,
    string Password);
}
