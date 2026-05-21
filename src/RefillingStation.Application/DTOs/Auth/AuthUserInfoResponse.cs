namespace RefillingStation.Application.DTOs.Auth
{
    public sealed record AuthUserInfoResponse(
        string UserId,
        string Username,
        string Role
    );
}
