using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Api.Controllers
{
    /// <summary>
    /// Controller for managing authenticated user account operations.
    /// Includes profile visualization, editing, and password changes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets the authenticated user's profile.
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "User ID inválido" });

            var response = await _userService.GetUserProfileAsync(userId);

            if (!response.Success)
                return BadRequest(new { success = false, message = response.Message });

            Log.Information(
                "User profile accessed: {@UserId} at {@AccessTime}",
                userId,
                DateTime.UtcNow
            );

            return Ok(new { success = true, data = response.Data });
        }

        /*
        /// <summary>
        /// Updates user profile data (PUT /api/user/profile)
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _userService.UpdateProfileAsync(int.Parse(userId), dto);
            return response.Success
                ? Ok(new { success = true, message = response.Message })
                : BadRequest(new { success = false, message = response.Message });
        }

        /// <summary>
        /// Changes user password (PATCH /api/user/change-password)
        /// </summary>
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _userService.ChangePasswordAuthenticatedAsync(
                int.Parse(userId),
                dto
            );
            return response.Success
                ? Ok(new { success = true, message = response.Message })
                : BadRequest(new { success = false, message = response.Message });
        }
        */
    }
}
