namespace TiendaUCN.src.Application.DTO.CartDTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; } = null!;
    }
}
