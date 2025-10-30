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
    /// Handles user authentication and JWT token generation.
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
        /// Login a user, validate credentials, and generate JWT.
        /// </summary>
        /// <param name="loginDto">User email, password, and RememberMe option.</param>
        /// <returns>GenericResponse with token and role or error message.</returns>
        public async Task<GenericResponse<object>> LoginAsync(LoginDTO loginDto)
        {
            // Log login attempt
            _logger.LogInformation("[LOGIN] Intento de login para {Email}", loginDto.Email);

            // Find user by email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("[LOGIN] Email no encontrado: {Email}", loginDto.Email);
                return new GenericResponse<object>("Credenciales inválidas");
            }

            // Check password
            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("[LOGIN] Contraseña incorrecta para {Email}", loginDto.Email);
                return new GenericResponse<object>("Credenciales inválidas");
            }

            // Get user role
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Cliente";

            // Generate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JWT key no configurada en appsettings.json");

            var key = Encoding.UTF8.GetBytes(keyString);
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
                    ? DateTime.UtcNow.AddHours(24) // RememberMe = 24h
                    : DateTime.UtcNow.AddHours(1), // Default = 1h
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };

            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            // Log successful login
            _logger.LogInformation(
                "[LOGIN] Inicio de sesión exitoso para {Email} con rol {Role}",
                loginDto.Email,
                role
            );

            // Return response
            return new GenericResponse<object>(
                "Inicio de sesión exitoso",
                new LoginResponseDTO { Token = token, Role = role }
            );
        }
    }
}
