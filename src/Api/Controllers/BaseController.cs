namespace TiendaUCN.src.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Base controller providing common functionality for all API controllers.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        // Common functionality for all controllers can be added here
    }
}