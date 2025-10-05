using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    /// <summary>
    /// Service interface for authentication operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user and returns a JWT token and role if successful.
        /// </summary>
        Task<GenericResponse<object>> LoginAsync(LoginDTO loginDto);
    }
}
