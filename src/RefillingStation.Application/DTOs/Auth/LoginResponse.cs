namespace RefillingStation.Application.DTOs.Auth
{
    public sealed record LoginResponse(
        string AccessToken,
        string RefreshToken,
        string Username,
        string Role
    );
}
