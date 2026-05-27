namespace RefillingStation.Application.DTOs.Common
{
    public sealed class DateRangeRequest
    {
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public int Page { get; init; } = 1; // optional
    }
}
