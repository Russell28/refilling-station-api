using FluentValidation;
using RefillingStation.Api.Contracts;
using RefillingStation.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace RefillingStation.Api.Middleware
{
    /// <summary>
    /// Centralized exception handler that converts all unhandled exceptions
    /// into clean, consistent JSON responses. This version includes
    /// exception-type mapping for proper HTTP status codes.
    /// </summary>
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while processing request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Maps known exception types to appropriate HTTP status codes
        /// and builds a standardized ErrorResponse object.
        /// </summary>
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var message = GetMessage(exception);
            var errors = GetErrors(exception);

            var response = new ErrorResponse
            {
                Message = message,
                Errors = errors,
                StatusCode = statusCode,
                Path = context.Request.Path
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }

        /// <summary>
        /// Determines the correct HTTP status code based on the exception type.
        /// </summary>
        private static int GetStatusCode(Exception ex) =>
            ex switch
            {
                // Validation errors (e.g., FluentValidation, DataAnnotations)
                ValidationException => (int)HttpStatusCode.BadRequest,
                NotFoundException => (int)HttpStatusCode.NotFound,
                ConflictException => (int)HttpStatusCode.Conflict,
                DomainException => (int)HttpStatusCode.UnprocessableEntity,

                // Unauthorized access
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,

                // Resource not found
                KeyNotFoundException => (int)HttpStatusCode.NotFound,

                // Domain or business rule violations
                InvalidOperationException => (int)HttpStatusCode.UnprocessableEntity,

                // Fallback for all other exceptions
                _ => (int)HttpStatusCode.InternalServerError
            };

        /// <summary>
        /// Provides a clean, user-friendly message based on the exception type.
        /// </summary>
        private static string GetMessage(Exception ex) =>
            ex switch
            {
                ValidationException => "Validation failed.",
                NotFoundException => ex.Message,
                ConflictException => ex.Message,
                DomainException => ex.Message,
                UnauthorizedAccessException => "Unauthorized request.",
                KeyNotFoundException => "Resource not found.",
                InvalidOperationException => "A business rule was violated.",
                _ => "An unexpected error occurred."
            };

        /// <summary>
        /// Extracts detailed validation errors when available.
        /// For other exception types, returns null.
        /// </summary>
        private static Dictionary<string, List<string>> GetErrors(Exception ex)
        {
            // FluentValidation → field-level errors
            if (ex is ValidationException validationEx)
            {
                return validationEx.Errors
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "general" : ToCamelCase(e.PropertyName))
                    .ToDictionary(
                        g => ToCamelCase(g.Key),
                        g => g.Select(e => e.ErrorMessage).ToList()
                    );
            }

            // ArgumentException → treat as general error
            if (ex is ArgumentException argEx)
            {
                return new Dictionary<string, List<string>>
                {
                    ["general"] = new() { argEx.Message }
                };
            }

            // Domain, Conflict, NotFound → general error
            if (ex is DomainException or ConflictException or NotFoundException)
            {
                return new Dictionary<string, List<string>>
                {
                    ["general"] = new() { ex.Message }
                };
            }

            // Fallback → general error
            return new Dictionary<string, List<string>>
            {
                ["general"] = new() { "An unexpected error occurred." }
            };
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
                return value;

            return char.ToLowerInvariant(value[0]) + value[1..];
        }
    }
}
