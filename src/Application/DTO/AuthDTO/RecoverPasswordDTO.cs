using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AuthDTO
{
    public class RecoverPasswordDTO
    {
        /// <summary>
        /// Gets or sets the user's email address.
        /// Must be a valid email format.
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
