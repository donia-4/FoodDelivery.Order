using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Order.Infrastructure.Services;

public static class HttpContentExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    public static async Task<T?> ReadFromEnvelopeAsync<T>(
        this HttpContent content,
        CancellationToken ct = default)
    {
        var envelope = await content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions, ct);

        if (envelope is null)
            return default;

        if (envelope.StatusCode is < 200 or >= 300)
            return default;

        return envelope.Data;
    }

    private record ApiEnvelope<T>(
        [property: JsonPropertyName("statusCode")] int StatusCode,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("errors")] object? Errors,
        [property: JsonPropertyName("data")] T Data);
}