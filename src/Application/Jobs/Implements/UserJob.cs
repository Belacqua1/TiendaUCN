using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Application.Jobs.Interface
{
    public class UserJob : IUserJob
    {
        private readonly IUserService _userService;

        public UserJob(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<int> DeleteUnconfirmedAsync()
        {
            return await _userService.DeleteUnconfirmedUsersAsync();
        }
    }
}