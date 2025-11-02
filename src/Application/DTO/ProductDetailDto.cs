namespace TiendaUCN.src.Application.DTO.Public
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public required string CategoryName { get; set; }
        public required string BrandName { get; set; }
        public ICollection<string> ImageUrls { get; set; } = new List<string>();
    }
}
