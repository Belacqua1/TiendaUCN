using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> DeleteUnconfirmedAsync();
        Task<GenericResponse<string>> RegisterAsync(
            RegisterDTO registerDto,
            string? clientIp = null
        );
    }
}
