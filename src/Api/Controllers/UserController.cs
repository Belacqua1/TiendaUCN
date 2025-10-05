using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVerificationService _verificationService;

        public AuthController(IUserService userService, IVerificationService verificationService)
        {
            _userService = userService;
            _verificationService = verificationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            // Extract client IP address
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Call the user service to register the user
            var response = await _userService.RegisterAsync(registerDto, clientIp);

            if (response.Message.Contains("ya existe") || response.Message.Contains("Error"))
                return BadRequest(new { success = false, message = response.Message });

            return Created(
                string.Empty,
                new
                {
                    success = true,
                    message = "Cuenta creada exitosamente. Se envió un código de verificación al correo.",
                }
            );
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyCodeDTO dto)
        {
            var success = await _verificationService.VerifyCodeAsync(dto.Email, dto.Code);
            if (!success)
                return BadRequest(new { message = "Código inválido o expirado." });

            return Ok(new { message = "Correo verificado correctamente." });
        }
    }
}
