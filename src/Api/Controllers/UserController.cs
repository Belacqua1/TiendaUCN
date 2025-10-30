using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TiendaUCN.src.Application.DTO.UserDTO;
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
        private readonly IVerificationService _verificationService;

        public UserController(IUserService userService, IVerificationService verificationService)
        {
            _userService = userService;
            _verificationService = verificationService;
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

        /// <summary>
        /// Updates user profile data (PUT /api/user/profile)
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // y el warning si el Claim no existiera o no fuera un número.
            if (!int.TryParse(userIdString, out int userId))
            {
                // El token es inválido o no contiene un ID de usuario válido.
                return Unauthorized(
                    new
                    {
                        success = false,
                        message = "Token inválido o ID de usuario no encontrado.",
                    }
                );
            }

            // Aquí sabes que 'userId' es un entero válido (ej: 101)
            var response = await _userService.UpdateProfileAsync(userId, dto);

            return response.Success
                ? Ok(new { success = true, message = response.Message })
                : BadRequest(new { success = false, message = response.Message });
        }

        /// <summary>
        /// Verifies a pending email change by checking the provided verification code.
        /// </summary>
        [HttpPost("verify-code-email")]
        public async Task<IActionResult> VerifyCodeEmail([FromBody] VerifyCodeEmail dto)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(
                    new
                    {
                        success = false,
                        message = "No se pudo obtener el correo del usuario autenticado.",
                    }
                );
            }
            var isValid = await _verificationService.VerifyPendingEmailAsync(userEmail, dto.Code);
            if (!isValid)
            {
                return BadRequest(
                    new { success = false, message = "Código de verificación inválido o expirado." }
                );
            }

            return Ok(new { success = true, message = "Código de verificación válido." });
        }

        /// <summary>
        /// Changes user password (PATCH /api/user/change-password)
        /// </summary>
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var response = await _userService.ChangePasswordAsync(userEmail, dto);
            return response.Success
                ? Ok(new { success = true, message = response.Message })
                : BadRequest(new { success = false, message = response.Message });
        }
    }
}
