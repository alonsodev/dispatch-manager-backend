using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DispatchManager.Application.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int LongRunningRequestThresholdMs = 3000;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > LongRunningRequestThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("Long running request detected: {RequestName} took {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
