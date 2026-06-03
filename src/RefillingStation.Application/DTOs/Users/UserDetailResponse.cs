namespace RefillingStation.Application.DTOs.Users
{
    public sealed record UserDetailResponse
    (
        int Id,
        string Username,
        string Role,
        bool IsActive
    );
}
