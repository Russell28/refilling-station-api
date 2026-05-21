using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.Interfaces.Services;

namespace RefillingStation.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            return Ok(result);
        }

        [HttpGet("active")]
        [AllowAnonymous] // removes AdminOnly
        [Authorize]      // requires authentication only
        public async Task<IActionResult> GetActive()
        {
            var result = await _service.GetActiveEmployeesAsync();

            return Ok(result);
        }
    }
}
