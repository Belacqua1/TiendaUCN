using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO.ProductsManagementDTO; // Asegúrate de importar tus DTOs

namespace TiendaUCN.src.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")] // R80: Protegido por rol de Administrador
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AdminProductsController : ControllerBase
    {
        // Aquí inyectarás tus servicios y DbContext
        // private readonly IProductAdminService _productService;
        // public AdminProductsController(IProductAdminService productService)
        // {
        //    _productService = productService;
        // }

        // --- Sub-flujo 6.1: CRUD ---

        [HttpPost]
        [ProducesResponseType(typeof(ProductAdminResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDTO createDto)
        {
            // R82: Validaciones (automáticas por DataAnnotations, pero faltan las de dominio)
            // R89: Validar existencia de brandId/categoryId
            // R83: Lógica para crear, setear IsActive=true, IsDeleted=false, CreatedAtUtc
            // ...
            // R83: Devolver 201 Created con el DTO de respuesta
            // var productDto = await _productService.CreateAsync(createDto);
            // return CreatedAtAction(nameof(GetProductById), new { id = productDto.Id }, productDto);

            return Ok("TODO: Implementar POST"); // Placeholder
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(
            Guid id,
            [FromBody] ProductUpdateDTO updateDto
        )
        {
            // R84: Lógica para buscar el producto (404 si no existe)
            // R82: Validaciones
            // R84: Actualizar campos, NO actualizar Id, CreatedAtUtc. Setear UpdatedAtUtc.
            // ...
            // await _productService.UpdateAsync(id, updateDto);
            // return NoContent(); // R84: 200/204 coherente

            return Ok("TODO: Implementar PUT"); // Placeholder
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            // R85: Eliminación Lógica (Soft Delete)
            // Buscar producto (404 si no existe)
            // NO BORRAR FÍSICAMENTE
            // Settear: product.IsDeleted = true; product.DeletedAtUtc = DateTime.UtcNow;
            // Guardar cambios
            // ...
            // await _productService.SoftDeleteAsync(id);
            // return NoContent(); // R85: 200/204 coherente

            return Ok("TODO: Implementar DELETE"); // Placeholder
        }

        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // R86: Debería ser un objeto de paginación
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] /* PagingParams */
            object queryParams
        )
        {
            // R86: Listado interno (con paginación, filtros, etc.)
            // Importante: Este listado SÍ debe poder incluir inactivos/eliminados si se pide.
            // ...
            // var pagedResult = await _productService.GetAllAdminAsync(queryParams);
            // return Ok(pagedResult);

            return Ok("TODO: Implementar GET (Listado)"); // Placeholder
        }

        [HttpGet("{id:guid}", Name = "GetProductById")] // Asignamos un nombre para el CreatedAtAction
        [ProducesResponseType(typeof(ProductAdminResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            // R87: Detalle interno
            // Buscar producto (incluyendo inactivos/eliminados)
            // 404 si no existe
            // Mapear a ProductAdminResponseDto (incluyendo IsActive, IsDeleted, etc.)
            // ...
            // var productDto = await _productService.GetByIdAdminAsync(id);
            // if (productDto == null) return NotFound();
            // return Ok(productDto);

            return Ok("TODO: Implementar GET (Detalle)"); // Placeholder
        }

        // --- Sub-flujo 6.2: Imágenes (Pendiente) ---
        // POST {id}/images
        // DELETE {id}/images/{imageId}

        // --- Sub-flujo 6.3: Estado y Descuento (Pendiente) ---
        // PATCH {id}/discount
        // PATCH {id}/status
    }
}
