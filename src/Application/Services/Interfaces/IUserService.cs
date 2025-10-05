using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for user-related services.
    /// Responsible for handling user registration and other user operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Registers a new user with the provided registration data.
        /// </summary>
        /// <param name="registerDto">A <see cref="RegisterDTO"/> containing the user's registration information.</param>
        /// <param name="clientIp">
        /// Optional parameter representing the IP address of the client performing the registration.
        /// Can be used for logging, auditing, or security purposes.
        /// </param>
        /// <returns>
        /// A <see cref="Task{GenericResponse}"/> containing a <see cref="GenericResponse{T}"/>
        /// with a message or result of the registration process.
        /// </returns>
        Task<GenericResponse<string>> RegisterAsync(
            RegisterDTO registerDto,
            string? clientIp = null
        );
    }
}
