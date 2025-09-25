Microsoft.AspNetCore.Mvc;

namespace TiendaUCN.src.Api.Controllers
{
    public class AuthController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var token = await _userService.LoginAsync(loginDTO);
        }
    }
}