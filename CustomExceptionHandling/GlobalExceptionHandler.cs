using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace CustomExceptionHandling;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly bool _isDevOrTestEnvironment = false;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        _isDevOrTestEnvironment = !String.IsNullOrEmpty(env) && (env.ToLower() == "development" || env.ToLower() == "test");
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken
    ) {
        var traceId = httpContext.TraceIdentifier;

        _logger.LogError(exception, "Exception occurred on {Machine}: ", Environment.MachineName);

        var problemDetails = MapToProblemDetails(exception, traceId);

        await Results.Problem(problemDetails).ExecuteAsync(httpContext);

        return true;
    }

    private ProblemDetails MapToProblemDetails(Exception ex, string? traceId)
    {
        var problemDetails = new ProblemDetails {
            Title = "Oops, something bad happened",
            Status = StatusCodes.Status500InternalServerError,
            Extensions = new Dictionary<string, object?> {
                {"traceId",  traceId},
                {"msg", ex.Message},
                {"error",ex.GetType().Name}
            }
        };

        return problemDetails;
    }
}
