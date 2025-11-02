using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.Public;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IProductAdminService
    {
        Task<ProductAdminResponseDTO> CreateAsync(ProductCreateDTO createDto);
        Task<ProductAdminResponseDTO?> GetByIdAdminAsync(int id);
        Task<PagedResponse<ProductAdminResponseDTO>> GetAllAdminAsync(
            ProductQueryParams queryParams
        );
        Task<bool> UpdateAsync(int id, ProductUpdateDTO updateDto);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> UpdateStatusAsync(int id, bool isAvailable);
        Task<bool> UpdateDiscountAsync(int id, int discount);
    }
}
