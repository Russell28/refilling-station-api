using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.Interfaces.Services;

namespace RefillingStation.Api.Controllers
{
    [Route("api/expense-categories")]
    [ApiController]
    [Authorize]
    public class ExpenseCategoriesController : ControllerBase
    {
        private readonly IExpenseCategoryService _service;

        public ExpenseCategoriesController(IExpenseCategoryService service)
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

        [HttpGet("list")]
        public async Task<IActionResult> GetCustomerList()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }
    }
}
