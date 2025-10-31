namespace TiendaUCN.src.Application.DTO.CartDTO
{
    public class CartItemDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public ProductDTO Product { get; set; } = null!;
        public int CartId { get; set; }
    }
}
