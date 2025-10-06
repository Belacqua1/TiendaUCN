using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.Product
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Description { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Debe indicar una categoría válida")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Debe indicar una marca válida")]
        public int BrandId { get; set; }
    }
}
