using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AuthDTO
{
    /// <summary>
    /// DTO for user login input data.
    /// </summary>
    public class LoginDTO
    {
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Determines if the user wants to stay logged in longer.
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}
