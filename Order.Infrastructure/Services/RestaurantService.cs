using System.Net.Http.Json;
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

        // بيقرأ الـ Envelope ويستخرج الـ Data
        var restaurant = await response.Content
            .ReadFromEnvelopeAsync<RestaurantResponse>(ct);

        return restaurant?.Name;
    }

    public async Task<MenuItemInfo?> GetMenuItemAsync(
        Guid menuItemId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(
            $"/api/menu-items/{menuItemId}", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var menuItem = await response.Content
            .ReadFromEnvelopeAsync<MenuItemResponse>(ct);

        if (menuItem is null)
            return null;

        return new MenuItemInfo(
            menuItemId, menuItem.RestaurantId, menuItem.Name, menuItem.Price, menuItem.IsAvailable);
    }

    private record RestaurantResponse(string Name);

    private record MenuItemResponse(Guid RestaurantId, string Name, decimal Price, bool IsAvailable);
}