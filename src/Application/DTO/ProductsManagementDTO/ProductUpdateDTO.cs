using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.ProductsManagementDTO
{
    /// <summary>
    /// DTO for user login input data.
    /// </summary>
    public class ProductUpdateDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public Guid BrandId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Range(1, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
