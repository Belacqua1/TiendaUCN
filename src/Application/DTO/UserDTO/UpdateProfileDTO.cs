using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.UserDTO
{
    public class UpdateProfileDTO
    {
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,50}$",
            ErrorMessage = "El nombre solo puede contener letras y espacios (2-50 caracteres)."
        )]
        public string? FirstName { get; set; }

        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,50}$",
            ErrorMessage = "El apellido solo puede contener letras y espacios (2-50 caracteres)."
        )]
        public string? LastName { get; set; }

        [RegularExpression(
            @"^(Masculino|Femenino|Otro)$",
            ErrorMessage = "El género debe ser Masculino, Femenino u Otro."
        )]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        [CustomValidation(typeof(UpdateProfileDTO), nameof(ValidateBirthDate))]
        public DateTime? BirthDate { get; set; }

        [RegularExpression(
            @"^\d{1,2}\.\d{3}\.\d{3}-[\dkK]{1}$",
            ErrorMessage = "El RUT debe tener un formato válido (ejemplo: 12.345.678-9)."
        )]
        public string? Rut { get; set; }

        [EmailAddress(ErrorMessage = "Debe ser un correo electrónico válido.")]
        public string? Email { get; set; }

        [RegularExpression(
            @"^\+569\d{8}$",
            ErrorMessage = "El teléfono debe tener el formato +569XXXXXXXX."
        )]
        public string? Phone { get; set; }

        public static ValidationResult? ValidateBirthDate(DateTime? date, ValidationContext context)
        {
            if (!date.HasValue)
                return ValidationResult.Success;
            if (date.Value > DateTime.Now)
                return new ValidationResult("La fecha de nacimiento no puede ser futura.");
            if (date.Value > DateTime.Now.AddYears(-18))
                return new ValidationResult("Debes ser mayor de 18 años.");
            return ValidationResult.Success;
        }
    }
}
