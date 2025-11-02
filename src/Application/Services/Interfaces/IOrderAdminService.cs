using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IOrderAdminService
    {
        Task<PagedResponse<OrderAdminListDto>> GetAllAsync(OrderQueryParams queryParams);
        Task<OrderAdminDetailDto?> GetByIdAsync(int orderId);
        Task UpdateStatusAsync(int orderId, string newStatus, int adminId);
    }
}
