using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.Public;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IPublicProductService
    {
        Task<PagedResponse<ProductListDto>> GetAllAsync(ProductQueryParams queryParams);
        Task<ProductDetailDto?> GetByIdAsync(int id);
    }
}
