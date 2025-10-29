using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements
{
    /// <summary>
    /// Service implements for token generation and management.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSecret =
                _configuration["JwtSettings:Secret"]
                ?? throw new ArgumentNullException("JWT Secret is not configured.");
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>A JWT token as a string.</returns>
        public Task<string> GenerateTokenAsync(User user, string role, bool rememberMe)
        {
            try
            {
                // Define list of claims that will be included in the token
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(ClaimTypes.Role, role),
                };
                // Create signing credentials using the secret key
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSecret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var tokenExpiration = rememberMe ? 24 : 1; // days
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(tokenExpiration),
                    signingCredentials: creds
                );
                Log.Information("JWT token generated successfully for user {UserId}", user.Id);
                return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                Log.Error("Error generating JWT token: {ErrorMessage}", ex.Message);

                throw InvalidOperationException("Error generating JWT token", ex);
            }
        }

        //implements in the middleware
        private Exception InvalidOperationException(string v, Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
