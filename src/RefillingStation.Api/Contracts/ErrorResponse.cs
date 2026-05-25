namespace RefillingStation.Api.Contracts
{
    public sealed class ErrorResponse
    {
        public bool Success { get; init; } = false;

        // High-level human-readable message
        public string Message { get; init; } = "An unexpected error occurred.";

        // Optional list of detailed validation or domain errors
        //public List<string>? Errors { get; init; }

        // Field-level or general errors
        // Key = field name (or "general")
        // Value = list of messages
        public Dictionary<string, List<string>> Errors { get; init; }
            = new();

        // HTTP status code returned by the API
        public int StatusCode { get; init; }

        // When the error occurred (UTC)
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        // The request path that triggered the error
        public string? Path { get; init; }
    }
}
