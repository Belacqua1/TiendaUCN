using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AdminDTO
{
    public class ProductUpdateDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Range(1, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        // BrandId y CategoryId no se suelen cambiar en una actualización, pero se podrían añadir si el negocio lo requiere.
    }
}
