using RefillingStation.Application.DTOs.Common;

namespace RefillingStation.Tests.Services.Builders
{
    public class DateRangeRequestBuilder
    {
        private DateOnly _start = DateOnly.FromDateTime(DateTime.UtcNow);
        private DateOnly _end = DateOnly.FromDateTime(DateTime.UtcNow);
        private int _page = 1;

        public DateRangeRequestBuilder WithStart(DateOnly start)
        {
            _start = start;
            return this;
        }

        public DateRangeRequestBuilder WithEnd(DateOnly end)
        {
            _end = end;
            return this;
        }

        public DateRangeRequest Build()
        {
            var request = new DateRangeRequest
            {
                StartDate = _start,
                EndDate = _end,
                Page = _page
            };

            return request;
        }
    }
}
