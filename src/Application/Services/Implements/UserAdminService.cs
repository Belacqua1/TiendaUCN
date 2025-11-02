using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Application.DTO.AdminDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.Exceptions;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class UserAdminService : IUserAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public UserAdminService(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<PagedResponse<UserAdminListDto>> GetAllUsersAsync(
            UserQueryParams queryParams
        )
        {
            var query = _context.Users.AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
                query = query.Where(u =>
                    u.Email.Contains(queryParams.SearchTerm)
                    || u.FirstName.Contains(queryParams.SearchTerm)
                    || u.LastName.Contains(queryParams.SearchTerm)
                );

            if (!string.IsNullOrWhiteSpace(queryParams.Status))
            {
                bool isLocked = queryParams.Status.ToLower() == "bloqueado";
                query = isLocked
                    ? query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
                    : query.Where(u =>
                        u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow
                    );
            }

            // El filtro por rol es más complejo y requiere un Join
            if (!string.IsNullOrWhiteSpace(queryParams.Role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(queryParams.Role);
                var userIdsInRole = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            var userDtos = new List<UserAdminListDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(
                    new UserAdminListDto
                    {
                        Id = user.Id,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Role = roles.FirstOrDefault() ?? "Sin Rol",
                        Status =
                            (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                                ? "Bloqueado"
                                : "Activo",
                        RegisteredAt = user.RegisteredAt,
                    }
                );
            }

            return new PagedResponse<UserAdminListDto>(
                userDtos,
                queryParams.PageNumber,
                queryParams.PageSize,
                totalCount
            );
        }

        public async Task<UserAdminDetailDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserAdminDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Sin Rol",
                Status =
                    (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
                        ? "Bloqueado"
                        : "Activo",
                Rut = user.Rut,
                PhoneNumber = user.PhoneNumber,
                RegisteredAt = user.RegisteredAt,
                LastLoginDate = user.LastLoginDate,
                EmailConfirmed = user.EmailConfirmed,
            };
        }

        public async Task UpdateUserStatusAsync(
            int adminId,
            int userIdToUpdate,
            bool isLocked,
            string reason
        )
        {
            // R136: No auto-bloqueo
            if (adminId == userIdToUpdate)
                throw new BusinessRuleException(
                    "Un administrador no puede bloquear su propia cuenta."
                );

            var userToUpdate = await _userManager.FindByIdAsync(userIdToUpdate.ToString());
            if (userToUpdate == null)
                throw new NotFoundException("Usuario a modificar no encontrado.");

            // R136: Protección del último Admin
            if (isLocked && await _userManager.IsInRoleAsync(userToUpdate, "Admin"))
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                if (adminUsers.Count <= 1)
                {
                    throw new ConflictException(
                        "No se puede bloquear al último administrador del sistema."
                    );
                }
            }

            string oldStatus =
                (userToUpdate.LockoutEnd != null && userToUpdate.LockoutEnd > DateTimeOffset.UtcNow)
                    ? "Bloqueado"
                    : "Activo";
            string newStatus = isLocked ? "Bloqueado" : "Activo";

            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(userToUpdate, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(userToUpdate, null);
            }

            // R137: Invalidar sesiones activas
            await _userManager.UpdateSecurityStampAsync(userToUpdate);

            // R138: Auditoría
            Log.Information(
                "AUDIT: El admin {AdminId} cambió el estado del usuario {UserId} de {OldStatus} a {NewStatus}. Razón: {Reason}",
                adminId,
                userIdToUpdate,
                oldStatus,
                newStatus,
                reason
            );
        }

        public async Task UpdateUserRoleAsync(int adminId, int userIdToUpdate, string newRole)
        {
            if (adminId == userIdToUpdate)
                throw new BusinessRuleException("Un administrador no puede cambiar su propio rol.");

            var userToUpdate = await _userManager.FindByIdAsync(userIdToUpdate.ToString());
            if (userToUpdate == null)
                throw new NotFoundException("Usuario a modificar no encontrado.");

            var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
            var currentRole = currentRoles.FirstOrDefault();

            if (currentRole == newRole)
                return; // No hay cambios

            // Regla implícita: Protección del último Admin
            if (currentRole == "Admin")
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                if (adminUsers.Count <= 1)
                {
                    throw new ConflictException(
                        "No se puede cambiar el rol del último administrador del sistema."
                    );
                }
            }

            var removeResult = await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
            if (!removeResult.Succeeded)
                throw new Exception("Error al remover el rol anterior.");

            var addResult = await _userManager.AddToRoleAsync(userToUpdate, newRole);
            if (!addResult.Succeeded)
                throw new Exception($"Error al añadir el nuevo rol '{newRole}'.");

            // Invalidar sesiones para forzar re-login con nuevos permisos
            await _userManager.UpdateSecurityStampAsync(userToUpdate);

            // Auditoría
            Log.Information(
                "AUDIT: El admin {AdminId} cambió el rol del usuario {UserId} de {OldRole} a {NewRole}.",
                adminId,
                userIdToUpdate,
                currentRole,
                newRole
            );
        }
    }
}
