using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;

namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IUserAdminService
    {
        Task<PagedResponse<UserAdminListDto>> GetAllUsersAsync(UserQueryParams queryParams);
        Task<UserAdminDetailDto?> GetUserByIdAsync(int userId);
        Task UpdateUserStatusAsync(int adminId, int userIdToUpdate, bool isLocked, string reason);
        Task UpdateUserRoleAsync(int adminId, int userIdToUpdate, string newRole);
    }
}
