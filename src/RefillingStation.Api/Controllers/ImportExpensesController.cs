using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.Interfaces.Services.Imports;

namespace RefillingStation.Api.Controllers
{
    [Route("api/expenses")]
    [ApiController]
    public class ImportExpensesController : ControllerBase
    {
        private readonly IExpenseImportService _service;

        public ImportExpensesController(IExpenseImportService service)
        {
            _service = service;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            // 1. Required
            if (file == null)
                return BadRequest("File is required.");

            // 2. Not empty
            if (file.Length == 0)
                return BadRequest("Uploaded file is empty.");

            // 3. Extension check
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".csv")
                return BadRequest("Only CSV files are allowed.");

            // Pass stream to service
            using var stream = file.OpenReadStream();
            var result = await _service.ImportAsync(stream);

            return Ok(result);
        }
    }
}
