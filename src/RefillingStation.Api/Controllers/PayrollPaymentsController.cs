using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.PayrollPayments;
using RefillingStation.Application.Interfaces.Services;

namespace RefillingStation.Api.Controllers
{
    [Route("api/payroll-payments")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class PayrollPaymentsController : ControllerBase
    {
        private readonly IPayrollPaymentService _service;

        public PayrollPaymentsController(IPayrollPaymentService service)
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
        public async Task<IActionResult> Create(PayrollPaymentCreateRequest request)
        {
            var id = await _service.CreateAsync(request);

            return Ok(id);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, PayrollPaymentCreateRequest request)
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
