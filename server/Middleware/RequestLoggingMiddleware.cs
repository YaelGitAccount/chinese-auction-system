using System.Diagnostics;

namespace server_NET.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];
            
            // Log request start
            _logger.LogInformation(
                "[{RequestId}] Starting {Method} {Path} from {RemoteIpAddress}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            );

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                // Log request completion
                var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
                
                _logger.Log(logLevel,
                    "[{RequestId}] Completed {Method} {Path} with {StatusCode} in {ElapsedMilliseconds}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );

                // Log slow requests
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "[{RequestId}] Slow request detected: {Method} {Path} took {ElapsedMilliseconds}ms",
                        requestId,
                        context.Request.Method,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds
                    );
                }
            }
        }
    }
}
