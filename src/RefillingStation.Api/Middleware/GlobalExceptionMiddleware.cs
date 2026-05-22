using RefillingStation.Api.Contracts;
using System.Net;
using System.Text.Json;

namespace RefillingStation.Api.Middleware
{
    /// <summary>
    /// A centralized middleware that catches ALL unhandled exceptions
    /// in the request pipeline. This ensures:
    /// - Consistent error responses
    /// - No raw exceptions leak to clients
    /// - All errors are logged in one place
    /// - Controllers remain clean (no try/catch)
    /// </summary>
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        /// <summary>
        /// Middleware is constructed with the next delegate in the pipeline
        /// and an ILogger instance for structured logging.
        /// </summary>
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// The main entry point for the middleware.
        /// Wraps the entire request pipeline in a try/catch.
        /// Any exception thrown by downstream components (controllers,
        /// services, EF Core, etc.) will be caught here.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Continue to the next middleware or controller
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception with full stack trace.
                // This is critical for debugging and production monitoring.
                _logger.LogError(ex, "Unhandled exception occurred while processing request.");

                // Convert the exception into a clean, safe API response.
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Converts an exception into a standardized JSON response.
        /// This basic version always returns 500 (Internal Server Error).
        /// More detailed exception mapping will be added in the next commit.
        /// </summary>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Default to 500 for unhandled exceptions.
            // This avoids leaking internal details to the client.
            var statusCode = (int)HttpStatusCode.InternalServerError;

            // Build a clean, predictable error response.
            // This ensures the frontend always receives the same structure.
            var response = new ErrorResponse
            {
                Message = "An unexpected error occurred.",
                StatusCode = statusCode,
                Path = context.Request.Path
            };

            // Configure the HTTP response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // Serialize using System.Text.Json for performance.
            // No stack traces or sensitive details are included.
            var json = JsonSerializer.Serialize(response);

            // Write the JSON payload to the response body.
            await context.Response.WriteAsync(json);
        }
    }
}
