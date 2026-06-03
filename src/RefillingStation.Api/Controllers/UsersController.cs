using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefillingStation.Application.DTOs.Users;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Domain.Enums;
using System.Security.Claims;

namespace RefillingStation.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _userService.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetByIdAsync(id);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateRequest request)
        {
            var result = await _userService.CreateAsync(request);

            return Ok(result);
        }

        [HttpPost("{id:int}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordRequest request)
        {
            await _userService.ChangePassword(id, request);
            return NoContent();
        }

        [HttpPost("{id:int}/change-role")]
        public async Task<IActionResult> ChangeRole(int id, ChangeUserRoleRequest request)
        {
            await _userService.ChangeRole(id, request);
            return NoContent();
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _userService.ActivateAsync(id);
            return NoContent();
        }

        [HttpPost("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _userService.DeactivateAsync(id, requestingUserId);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _userService.DeleteAsync(id, requestingUserId);
            return NoContent(); // 204 No Content is the RESTful standard for delete
        }
    }
}
