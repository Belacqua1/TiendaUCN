using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> CreateAsync(Product product);
        Task<Product?> GetByIdAsync(int id);
    }
}
