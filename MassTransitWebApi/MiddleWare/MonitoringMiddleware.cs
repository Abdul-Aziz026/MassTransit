namespace MassTransitWebApi.Middleware;

public class MonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MonitoringMiddleware> _logger;

    public MonitoringMiddleware(RequestDelegate next, ILogger<MonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopWatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopWatch.Stop();
            _logger.LogInformation("Request {Method} {context.Request.Path} completed in {stopWatch.ElapsedMilliseconds}ms with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                stopWatch.ElapsedMilliseconds,
                context.Response.StatusCode);
        }
    }
}