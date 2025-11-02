using TiendaUCN.src.Application.DTO;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.Public;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface ICategoryAdminService
    {
        Task<PagedResponse<TaxonomyResponseDto>> GetAllAsync(ProductQueryParams queryParams);
        Task<TaxonomyResponseDto> CreateAsync(TaxonomyCreateDto dto);
        Task<TaxonomyResponseDto> UpdateAsync(int id, TaxonomyUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
