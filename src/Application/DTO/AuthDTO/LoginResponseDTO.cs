namespace TiendaUCN.src.Application.DTO.AuthDTO
{
    /// <summary>
    /// DTO for returning login results.
    /// Contains the JWT token and the role of the user.
    /// </summary>
    public class LoginResponseDTO
    {
        // JWT token issued for authenticated user
        public string Token { get; set; } = string.Empty;

        // Role of the authenticated user
        public string Role { get; set; } = string.Empty;
    }
}
