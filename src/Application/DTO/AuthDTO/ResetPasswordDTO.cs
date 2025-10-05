using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AuthDTO
{
    public class ResetPasswordDTO
    {
        /// <summary>
        /// Gets or sets the user's email address.
        /// Must be a valid email format.
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the verification code sent to the user's email.
        /// Initialized as an empty string to avoid null reference issues.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's password.
        /// Must have at least 8 characters, uppercase, lowercase, number, and special character.
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres, mayúscula, minúscula, número y carácter especial."
        )]
        public string NewPassword { get; set; } = string.Empty;
    }
}
