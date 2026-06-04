using Microsoft.AspNetCore.Http;
using RefillingStation.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RefillingStation.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _service.LoginAsync(request);

            return Ok(result);
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var username = User.Identity?.Name;

            var role = User.FindFirstValue(
                ClaimTypes.Role);

            if (userId is null ||
                username is null ||
                role is null)
            {
                return Unauthorized();
            }

            return Ok(new AuthUserInfoResponse(
                userId,
                username,
                role
            ));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _service.RefreshTokenAsync(request);

            return Ok(result);
        }
    }
}
