using Microsoft.AspNetCore.Mvc;
using RefillingStation.Domain.Enums;

namespace RefillingStation.Api.Controllers
{
    [Route("api/employee-roles")]
    [ApiController]
    public class EmployeeRolesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var roles = Enum.GetValues<EmployeeRole>()
                .Cast<EmployeeRole>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = e.ToString()
                });

            return Ok(roles);
        }
    }
}
