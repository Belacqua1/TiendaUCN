using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.Product;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<GenericResponse<ProductResponseDto>> CreateAsync(ProductCreateDto dto);
        Task<GenericResponse<ProductResponseDto>> GetByIdAsync(int id);
    }
}
