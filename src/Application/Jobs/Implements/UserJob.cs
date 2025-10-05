using Hangfire;
using Serilog;
using TiendaUCN.src.Application.Jobs.Interfaces;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Application.Jobs.implements
{
    /// <summary>
    /// Clase para manejar trabajos de usuario con Hangfire.
    /// </summary>
    public class UserJob : IUserJob
    {
        private readonly IUserService _userService;

        public UserJob(IUserService userService, IConfiguration _configuration)
        {
            _userService = userService;
        }
    }
}
