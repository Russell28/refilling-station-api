namespace RefillingStation.Application.DTOs.Trips
{
    public sealed record NextTripNumberResponse(
        DateOnly Date,
        int NextTripNo
    );
}
