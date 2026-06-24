using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.DTOs.Employees;
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

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateRequest request)
        {
            await _service.CreateAsync(request);
            return Created();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, EmployeeCreateRequest request)
        {
            await _service.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _service.ActivateAsync(id);
            return NoContent();
        }

        [HttpPost("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _service.DeactivateAsync(id);
            return NoContent();
        }
    }
}
