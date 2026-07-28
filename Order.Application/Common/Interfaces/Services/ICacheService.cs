namespace Order.Application.Common.Interfaces.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken ct = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        string[]? tags = null,
        CancellationToken ct = default);

    Task RemoveAsync(
        string key,
        CancellationToken ct = default);

    Task RemoveByTagAsync(
        string tag,
        CancellationToken ct = default);
}