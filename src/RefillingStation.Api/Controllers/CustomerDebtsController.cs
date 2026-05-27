using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.CustomerDebts;
using RefillingStation.Application.Interfaces.Services;

namespace RefillingStation.Api.Controllers
{
    [Route("api/customer-debts")]
    [ApiController]
    [Authorize]
    public class CustomerDebtsController : ControllerBase
    {
        private readonly ICustomerDebtService _service;

        public CustomerDebtsController(ICustomerDebtService service)
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

        [HttpPost]
        public async Task<IActionResult> Create(CustomerDebtCreateRequest request)
        {
            var id = await _service.CreateAsync(request);

            return Ok(id);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CustomerDebtCreateRequest request)
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

        [HttpPost("search")]
        public async Task<IActionResult> Search(DateRangeRequest request)
        {
            var result = await _service.SearchByDateRangeAsync(request);

            return Ok(result);
        }
    }
}
