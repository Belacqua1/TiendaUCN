namespace TiendaUCN.src.Application.DTO.CartDTO
{
    public class CartDTO
    {
        public int Id { get; set; }
        public int Total { get; set; }
        public int SubTotal { get; set; }
        public string BuyerId { get; set; } = null!;
        public int? UserId { get; set; }
        public List<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
