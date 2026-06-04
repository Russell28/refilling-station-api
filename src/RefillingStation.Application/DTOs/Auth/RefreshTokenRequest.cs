namespace RefillingStation.Application.DTOs.Auth
{
    public sealed class RefreshTokenRequest
    {
        public string Token { get; init; } = string.Empty;
    }
}
