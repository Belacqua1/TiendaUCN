using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.ProductsManagementDTO
{
    /// <summary>
    /// DTO for creating a new product.
    /// </summary>
    public class ProductAdminResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // Datos de admin
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public decimal DiscountPercent { get; set; } // Para R97
        public decimal FinalPrice { get; set; } // Para R97 (calculado en servidor)

        // IDs de relaciones
        public Guid BrandId { get; set; }
        public Guid CategoryId { get; set; }

        // Opcional: DTOs anidados de relaciones
        // public BrandDto Brand { get; set; }
        // public CategoryDto Category { get; set; }
        // public List<ProductImageDto> Images { get; set; }
    }
}
