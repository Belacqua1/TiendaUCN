using Bogus.DataSets;

namespace TiendaUCN.src.Domain.Models
{
    public class Image
    {
        public int Id { get; set; }
        public required string ImageUrl { get; set; }
        public required string PublicId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Identificador del producto asociado a la imagen.
        /// </summary>
        public int ProductId { get; set; }
    }
}
