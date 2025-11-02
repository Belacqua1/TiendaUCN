using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AdminDTO
{
    /// <summary>
    /// DTO for creating a new product.
    /// </summary>
    public class ProductCreateDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Range(1, double.MaxValue)] // R82: price > 0
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)] // R82: stock >= 0
        public int Stock { get; set; }
    }
}

// DTO para actualizar un producto (R81)
// Nota: Para PUT, todos los campos son requeridos. Si usaras PATCH, serían opcionales.
// DTO de respuesta para el admin (R87)
// Este DTO SÍ incluye estado, descuentos, etc.
