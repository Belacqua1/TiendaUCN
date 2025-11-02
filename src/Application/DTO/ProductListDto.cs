namespace TiendaUCN.src.Application.DTO.Public
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public required string MainImageUrl { get; set; }
        public required string CategoryName { get; set; }
    }
}
