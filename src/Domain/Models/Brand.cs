using Bogus.DataSets;

namespace TiendaUCN.src.Domain.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string Slug { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
