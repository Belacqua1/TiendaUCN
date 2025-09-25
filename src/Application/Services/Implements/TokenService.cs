using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Serilog;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Aplication.Services.Implements
{
    public class TokenService : ITokenService
    {
        // Token service implementation
        private readonly IConfiguration _configuration;
        private readonly required string _jwtSecret;
        public TokenService(IConfiguration configuration)
        {
            _configuration =
                configuration
                ?? throw new ArgumentNullException(
                    nameof(configuration),
                    "The configuration cannot be null."
                );
            _jwtSecret =
                _configuration["JWTSecret"]
                ?? throw new InvalidOperationException("Jwt:Secret configuration is missing");
        }
        public string GenerateToken(User user, string roleName, bool rememberMe)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Role, roleName),
                };

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSecret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(rememberMe ? 24 : 1),
                    signingCredentials: creds
                );
                Log.Information("Token generated successfully for user {UserId}", user.Id);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error to generate JWT token for user {UserId}", user.Id);
                throw new InvalidOperationException(
                    "An error occurred while generating the token.",
                    ex
                );
            }
        }
    }
}
