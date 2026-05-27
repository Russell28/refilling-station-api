namespace RefillingStation.Application.DTOs.Common
{
    public sealed class DateRangeOptions
    {
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public int Page { get; init; }
    }
}
