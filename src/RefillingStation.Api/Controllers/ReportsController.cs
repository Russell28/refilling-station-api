using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.Interfaces.Services;

namespace RefillingStation.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetDashboard(DateTime startDate, DateTime endDate)
        {
            var result = await _reportsService.GetDashboardAsync(startDate, endDate);

            return Ok(result);
        }

        [HttpGet("daily-summary/{date:datetime}")]
        public async Task<IActionResult> GetDailySummary(DateTime date)
        {
            var result = await _reportsService.GetDailySummaryAsync(date);

            return Ok(result);
        }

        [HttpGet("monthly-summary/{monthYear}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetMonthlySummary(string monthYear)
        {
            var result = await _reportsService.GetMonthlySummaryAsync(monthYear);

            return Ok(result);
        }
    }
}
