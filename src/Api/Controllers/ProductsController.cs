using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO; // Asegúrate de que este using esté presente si es necesario para otros DTOs, o elimínalo si no.
using TiendaUCN.src.Application.DTO.Public;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Api.Controllers.Public
{
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IPublicProductService _publicProductService;

        public ProductsController(IPublicProductService publicProductService)
        {
            _publicProductService = publicProductService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ProductListDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParams queryParams)
        {
            var pagedResult = await _publicProductService.GetAllAsync(queryParams);
            // R67: Añadir metadatos de paginación a las cabeceras
            Response.Headers.Append("X-Pagination-TotalCount", pagedResult.TotalCount.ToString());
            Response.Headers.Append("X-Pagination-TotalPages", pagedResult.TotalPages.ToString());
            Response.Headers.Append("X-Pagination-CurrentPage", pagedResult.CurrentPage.ToString());
            Response.Headers.Append("X-Pagination-PageSize", pagedResult.PageSize.ToString());

            return Ok(pagedResult.Items);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _publicProductService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }
    }
}
