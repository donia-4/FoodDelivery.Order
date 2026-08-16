using System.Text.Json.Serialization;
using Order.Application.Common.Interfaces.Services;
using Order.Infrastructure.Services;

namespace Order.Infrastructure.Services;

public sealed class RestaurantService : IRestaurantService
{
    private readonly HttpClient _httpClient;

    public RestaurantService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("RestaurantService");
    }

    public async Task<string?> GetRestaurantNameAsync(
        Guid restaurantId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(
            $"/api/restaurants/{restaurantId}", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var restaurant = await response.Content
            .ReadFromEnvelopeAsync<RestaurantResponse>(ct);

        return restaurant?.Name;
    }

    public async Task<MenuItemInfo?> GetMenuItemAsync(
        Guid menuItemId, CancellationToken ct = default)
    {
        // ✅ التصحيح هنا: الـ endpoint الحقيقي هو /api/foods/{id}
        var response = await _httpClient.GetAsync(
            $"/api/foods/{menuItemId}", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var menuItem = await response.Content
            .ReadFromEnvelopeAsync<MenuItemResponse>(ct);

        if (menuItem is null)
            return null;

        return new MenuItemInfo(
            menuItemId,
            menuItem.RestaurantId,
            menuItem.Name,
            menuItem.Price,
            menuItem.IsAvailable);
    }

    private record RestaurantResponse(
        [property: JsonPropertyName("name")] string Name);

    private record MenuItemResponse(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("restaurantId")] Guid RestaurantId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("price")] decimal Price,
        [property: JsonPropertyName("isAvailable")] bool IsAvailable);
}