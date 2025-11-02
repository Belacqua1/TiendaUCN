using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AdminDTO
{
    // DTO para la lista de usuarios
    public class UserAdminListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; } // "Activo", "Bloqueado"
        public DateTime RegisteredAt { get; set; }
    }

    // DTO para el detalle de un usuario
    public class UserAdminDetailDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string Rut { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    // DTO para actualizar el estado (bloqueo)
    public class UserStatusUpdateDto
    {
        [Required]
        public bool IsLocked { get; set; }
        public string Reason { get; set; } // Para auditoría
    }

    // DTO para actualizar el rol
    public class UserRoleUpdateDto
    {
        [Required]
        public string NewRole { get; set; }
    }

    // DTO para los parámetros de consulta
    public class UserQueryParams : ProductQueryParams // Reutilizamos la paginación
    {
        public string? Role { get; set; }
        public string? Status { get; set; }
    }
}
