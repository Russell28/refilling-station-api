using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.DTOs.MonthlyClosing;
using RefillingStation.Application.Interfaces.Services;

namespace RefillingStation.Api.Controllers
{
    [Route("api/monthly-closings")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class MonthlyClosingController : ControllerBase
    {
        private readonly IMonthlyClosingService _service;

        public MonthlyClosingController(IMonthlyClosingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdate(MonthlyClosingRequest request)
        {
            var result = await _service.CreateOrUpdateAsync(request);

            return Ok(result);
        }
    }
}
