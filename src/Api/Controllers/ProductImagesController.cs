using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.Services.Interfaces;

namespace TiendaUCN.src.Api.Controllers
{
    [ApiController]
    [Route("api/admin/products/{productId}/images")]
    [Authorize(Roles = "Administrador")]
    public class ProductImagesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<ProductImagesController> _logger;

        public ProductImagesController(
            IFileService fileService,
            ILogger<ProductImagesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(int productId, IFormFile file)
        {
            try
            {
                _logger.LogInformation("📤 Iniciando subida de imagen para producto: {ProductId}", productId);

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No se proporcionó ningún archivo" });

                var result = await _fileService.UploadAsync(file, productId);

                if (result)
                {
                    _logger.LogInformation("✅ Imagen subida exitosamente para producto: {ProductId}", productId);
                    return Ok(new { 
                        message = "Imagen subida exitosamente",
                        productId = productId
                    });
                }
                else
                {
                    return Conflict(new { message = "La imagen ya existe" });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("❌ Error de validación: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error al subir imagen para producto: {ProductId}", productId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}