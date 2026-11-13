namespace Api_pendejos.DTO
{
    public record CategoryRequest(string Name);

    public record CartAddRequest(int ProductId, int Quantity = 1);
    public record CartUpdateRequest(int Quantity);

    public record FavoriteAddRequest(int ProductId);

    public record PickupPointRequest(string Name, string Address);

    public record DirectOrderRequest(
        int ProductId,
        int Quantity = 1,
        string DeliveryType = "Самовывоз",
        string? DeliveryAddress = null,
        int? PickupPointId = null);

    public record CartOrderRequest(
        string DeliveryType,
        string? DeliveryAddress = null,
        int? PickupPointId = null);

    public record OrderStatusRequest(int StatusId);

    public record ReviewCreateRequest(int ProductId, int Rating, string? Comment);
    public record ReviewUpdateRequest(int Rating, string? Comment);
}