using TiendaUCN.Domain.Models;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public class ITokenService
    {
        string GenerateToken(User user, string roleName, bool rememberMe);
    }
}
