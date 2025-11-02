using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.Public;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")] // R80: Protegido por rol de Administrador
    [Produces("application/json")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductAdminService _productAdminService;
        private readonly IImageService _imageService;

        public AdminProductsController(
            IProductAdminService productAdminService,
            IImageService imageService
        )
        {
            _productAdminService = productAdminService;
            _imageService = imageService;
        }

        // --- Sub-flujo 6.1: CRUD ---

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ProductAdminResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDTO createDto)
        {
            try
            {
                var productDto = await _productAdminService.CreateAsync(createDto);
                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = productDto.Id },
                    productDto
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(
            int id,
            [FromBody] ProductUpdateDTO updateDto
        )
        {
            var success = await _productAdminService.UpdateAsync(id, updateDto);
            if (!success)
            {
                return NotFound(new { message = "Producto no encontrado." });
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productAdminService.SoftDeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Producto no encontrado." });
            }
            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(PagedResponse<ProductAdminResponseDTO>),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParams queryParams)
        {
            var pagedResult = await _productAdminService.GetAllAdminAsync(queryParams);
            return Ok(pagedResult);
        }

        [HttpGet("{id:int}", Name = "GetProductById")]
        [ProducesResponseType(typeof(ProductAdminResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id)
        {
            var productDto = await _productAdminService.GetByIdAdminAsync(id);
            if (productDto == null)
            {
                return NotFound(new { message = "Producto no encontrado." });
            }
            return Ok(productDto);
        }

        // --- Sub-flujo 6.2: Imágenes ---

        [HttpPost("{id:int}/images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            try
            {
                var success = await _imageService.UploadImageAsync(file, id);
                if (!success)
                {
                    return BadRequest(new { message = "No se pudo subir la imagen." });
                }
                return Ok(new { message = "Imagen subida correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}/images/{publicId}")]
        public async Task<IActionResult> DeleteImage(int id, string publicId)
        {
            // El 'id' del producto no se usa en la lógica de borrado de imagen, pero es bueno para la estructura del endpoint.
            var success = await _imageService.DeleteImageAsync(publicId);
            if (!success)
            {
                return NotFound(new { message = "Imagen no encontrada." });
            }
            return NoContent();
        }

        // --- Sub-flujo 6.3: Estado y Descuento ---

        [HttpPatch("{id:int}/status")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateProductStatus(
            int id,
            [FromBody] ProductStatusUpdateDTO statusDto
        )
        {
            var success = await _productAdminService.UpdateStatusAsync(id, statusDto.IsAvailable);
            if (!success)
                return NotFound(new { message = "Producto no encontrado o ya está eliminado." });
            return NoContent();
        }

        [HttpPatch("{id:int}/discount")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateProductDiscount(
            int id,
            [FromBody] ProductDiscountUpdateDTO discountDto
        )
        {
            var success = await _productAdminService.UpdateDiscountAsync(id, discountDto.Discount);
            if (!success)
                return NotFound(new { message = "Producto no encontrado." });
            return NoContent();
        }
    }
}
