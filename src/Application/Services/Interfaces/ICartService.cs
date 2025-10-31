namespace TiendaUCN.src.Application.Services.Interfaces
{
    using TiendaUCN.src.Application.DTO.CartDTO;
    using TiendaUCN.src.Domain.Models;

    public interface ICartService
    {
        Task<CartDTO> GetCartAsync();
        Task<CartDTO> AddItemAsync(AddCartItemDTO addCartItemDTO);
        Task RemoveItemAsync(int productId);
        Task<CartDTO> UpdateItemQuantityAsync(int productId, int quantity);
        Task<CartDTO> CheckoutAsync();
    }
}
