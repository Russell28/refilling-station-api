using Microsoft.AspNetCore.Mvc;
using RefillingStation.Domain.Enums;

namespace RefillingStation.Api.Controllers
{
    [Route("api/employment-types")]
    [ApiController]
    public class EmploymentTypesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var types = Enum.GetValues<EmploymentType>()
                .Cast<EmploymentType>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = e.ToString()
                });

            return Ok(types);
        }
    }
}
