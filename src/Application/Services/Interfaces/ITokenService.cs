using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    /// <summary>
    /// Service interface for token generation and management.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>A JWT token as a string.</returns>
        Task<string> GenerateTokenAsync(User user, string role, bool rememberMe);
    }
}
