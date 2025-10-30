using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Api.Controllers
{
    /// <summary>
    /// Controller responsible for user authentication and registration endpoints.
    /// Handles registration, email verification, and related operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVerificationService _verificationService;
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userService">Service to handle user-related operations.</param>
        /// <param name="verificationService">Service to handle email verification logic.</param>
        public AuthController(
            IUserService userService,
            IVerificationService verificationService,
            IAuthService authService
        )
        {
            _userService = userService;
            _verificationService = verificationService;
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user.
        /// Validates input, creates the user, assigns a role, and sends a verification code.
        /// </summary>
        /// <param name="registerDto">Data transfer object containing registration information.</param>
        /// <returns>
        /// Returns a 201 Created with success message if registration succeeds.
        /// Returns a 400 BadRequest if the email or RUT already exists, or if an error occurs.
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            // Extract client IP address for logging or auditing purposes
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Call the user service to register the user
            var response = await _userService.RegisterAsync(registerDto, clientIp);

            // Check for duplicate or error messages
            if (response.Message.Contains("ya existe") || response.Message.Contains("Error"))
                return BadRequest(new { success = false, message = response.Message });

            // Return success response with information about verification code
            return Created(
                string.Empty,
                new
                {
                    success = true,
                    message = "Cuenta creada exitosamente. Se envió un código de verificación al correo.",
                }
            );
        }

        /// <summary>
        /// Verifies a user's email using a previously sent verification code.
        /// Activates the account if the code is valid and sends a welcome email.
        /// </summary>
        /// <param name="dto">Data transfer object containing the email and verification code.</param>
        /// <returns>
        /// Returns a 200 OK if the code is valid.
        /// Returns a 400 BadRequest if the code is invalid or expired.
        /// </returns>
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyCodeDTO dto)
        {
            // Attempt to verify the code using the verification service
            var success = await _verificationService.VerifyCodeAsync(dto.Email, dto.Code);

            if (!success)
                return BadRequest(new { message = "Código inválido o expirado." });

            return Ok(new { message = "Correo verificado correctamente." });
        }

        /// <summary>
        /// User login endpoint.
        /// </summary>
        /// <param name="loginDto">Email, password and RememberMe flag.</param>
        /// <returns>
        /// GenericResponse with JWT and role if successful,
        /// or an error message if login fails.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            // Call the AuthService to perform login
            var response = await _authService.LoginAsync(loginDto);

            // Check if response.Data contains a LoginResponseDTO
            if (response.Data is not LoginResponseDTO data)
                return Unauthorized(new { success = false, message = response.Message });

            // Return token and role for frontend redirection according to user role
            return Ok(
                new
                {
                    success = true,
                    message = response.Message,
                    token = data.Token,
                    role = data.Role,
                }
            );
        }

        [HttpPost("recover-password")]
        public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordDTO dto)
        {
            var response = await _userService.RecoverPasswordAsync(dto);
            if (!response.Success)
                return BadRequest(new { success = false, message = response.Message });

            return Ok(new { success = true, message = response.Message });
        }

        [HttpPatch("reset-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ResetPasswordDTO dto)
        {
            var response = await _userService.ChangePasswordAsync(dto);
            if (!response.Success)
                return BadRequest(new { success = false, message = response.Message });

            return Ok(new { success = true, message = response.Message });
        }
    }
}
