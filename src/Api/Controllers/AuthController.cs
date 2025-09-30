namespace TiendaUCN.src.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            // Extract client IP address
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Call the user service to register the user
            var response = await _userService.RegisterAsync(registerDto, clientIp);

            if (response.Message.Contains("ya existe") || response.Message.Contains("Error"))
                return BadRequest(response);

            return Ok(response);
        }
    }
}
