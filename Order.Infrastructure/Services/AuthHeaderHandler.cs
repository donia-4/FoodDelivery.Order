using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Order.Infrastructure.Services;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader))
            {
                request.Headers.Authorization =
                    AuthenticationHeaderValue.Parse(authHeader);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}