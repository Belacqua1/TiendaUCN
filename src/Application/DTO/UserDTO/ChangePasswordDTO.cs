using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.UserDTO
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres, mayúscula, minúscula, número y carácter especial."
        )]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres, mayúscula, minúscula, número y carácter especial."
        )]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare("NewPassword", ErrorMessage = "La confirmación de contraseña no coincide.")]
        public required string ConfirmNewPassword { get; set; }
    }
}
