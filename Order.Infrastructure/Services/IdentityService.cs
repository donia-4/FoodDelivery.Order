using System.Net.Http.Json;
using Order.Application.Common.Interfaces.Services;

namespace Order.Infrastructure.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly HttpClient _httpClient;

    public IdentityService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("IdentityService");
    }

    public async Task<(string Name, string Phone)?> GetUserAsync(
        Guid userId, CancellationToken ct = default)
    {
        // مباشرة للـ Identity Service: /api/users/{id}
        var response = await _httpClient.GetAsync(
            $"/api/users/{userId}", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var user = await response.Content
            .ReadFromJsonAsync<UserResponse>(ct);

        return user is null ? null : (user.FullName, user.PhoneNumber);
    }

    private record UserResponse(
        string FullName,
        string PhoneNumber);
}