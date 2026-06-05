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

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _service.LoginAsync(request);

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                accessToken = result.AccessToken
            });
        }

        [HttpGet("me")]
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

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                accessToken = result.AccessToken
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID claim is missing.");

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user ID claim.");

            await _service.LogoutAsync(userId);

            HttpContext.Response.Cookies.Delete("refreshToken");

            return Ok();
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            HttpContext.Response.Cookies.Append(
                "refreshToken",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,              // use true in production (requires HTTPS)
                    SameSite = SameSiteMode.None, // allow cross‑origin requests
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                });
        }
    }
}
