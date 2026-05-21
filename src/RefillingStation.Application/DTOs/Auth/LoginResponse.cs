namespace RefillingStation.Application.DTOs.Auth
{
    public sealed record LoginResponse(
        string Token,
        string Username,
        string Role
    );
}
