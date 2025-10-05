using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements
{
    /// <summary>
    /// Handles user authentication and JWT generation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            IConfiguration configuration,
            ILogger<AuthService> logger
        )
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="loginDto">Login DTO containing email, password, and RememberMe option.</param>
        /// <returns>GenericResponse with token and user role if successful, or error message if failed.</returns>
        public async Task<GenericResponse<object>> LoginAsync(LoginDTO loginDto)
        {
            _logger.LogInformation("[LOGIN] Intento de login para {Email}", loginDto.Email);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("[LOGIN] Email no encontrado: {Email}", loginDto.Email);
                return new GenericResponse<object>("Credenciales inválidas");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("[LOGIN] Contraseña incorrecta para {Email}", loginDto.Email);
                return new GenericResponse<object>("Credenciales inválidas");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Cliente";

            // Generar JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWTSecret"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(ClaimTypes.Role, role),
                    }
                ),
                Expires = loginDto.RememberMe
                    ? DateTime.UtcNow.AddHours(24)
                    : DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };

            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            _logger.LogInformation(
                "[LOGIN] Inicio de sesión exitoso para {Email} con rol {Role}",
                loginDto.Email,
                role
            );

            return new GenericResponse<object>(
                "Inicio de sesión exitoso",
                new LoginResponseDTO { Token = token, Role = role }
            );
        }
    }
}
