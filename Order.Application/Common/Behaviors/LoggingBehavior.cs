using MediatR;
using Microsoft.Extensions.Logging;

namespace Order.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling {RequestName}",
            typeof(TRequest).Name);

        var response = await next();

        _logger.LogInformation(
            "Handled {RequestName}",
            typeof(TRequest).Name);

        return response;
    }
}