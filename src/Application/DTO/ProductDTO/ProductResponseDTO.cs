namespace TiendaUCN.src.Application.DTO.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string CategoryName { get; set; } = null!;
        public string BrandName { get; set; } = null!;
    }
}
