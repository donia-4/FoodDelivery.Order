using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain.Results.Abstractions;
using Order.Application.Common.Interfaces;
using Order.Application.Common.Interfaces.Services;
namespace Order.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    ICacheService cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next();
        }

        _logger.LogInformation(
            "Checking cache for {Request}",
            typeof(TRequest).Name);

        var cached = await _cache.GetAsync<TResponse>(
            cachedRequest.CacheKey,
            cancellationToken);

        if (cached is not null)
        {
            _logger.LogInformation(
                "Cache hit for {Request}",
                typeof(TRequest).Name);

            return cached;
        }

        _logger.LogInformation(
            "Cache miss for {Request}",
            typeof(TRequest).Name);

        var response = await next();

        if (response is IResult result && result.IsSuccess)
        {
            await _cache.SetAsync(
                cachedRequest.CacheKey,
                response,
                cachedRequest.Expiration,
                cachedRequest.Tags,
                cancellationToken);
        }

        return response;
    }
}