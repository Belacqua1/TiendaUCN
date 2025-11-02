using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AdminDTO
{
    public class TaxonomyCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class TaxonomyUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class TaxonomyResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Slug { get; set; } = null!;
        public int ProductCount { get; set; }
    }
}
