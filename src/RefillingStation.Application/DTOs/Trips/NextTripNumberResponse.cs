namespace RefillingStation.Application.DTOs.Trips
{
    public record NextTripNumberResponse(
        DateOnly Date,
        int NextTripNo
    );
}
