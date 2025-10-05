namespace TiendaUCN.src.Application.DTO.AuthDTO
{
    /// <summary>
    /// Response DTO for successful login.
    /// Contains the JWT token and the user's role.
    /// </summary>
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
