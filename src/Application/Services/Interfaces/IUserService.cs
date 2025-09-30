namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<GenericResponse<string>> RegisterAsync(Register registerDto, string? clientIp = null);
    }
}
