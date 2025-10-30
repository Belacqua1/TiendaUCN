using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTO.AuthDTO
{
    /// <summary>
    /// Data transfer object for registering a new user.
    /// Includes validation rules for all user properties.
    /// </summary>
    public class RegisterDTO
    {
        /// <summary>
        /// Gets or sets the user's first name.
        /// Only letters and spaces are allowed (2-50 characters).
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,50}$",
            ErrorMessage = "El nombre solo puede contener letras y espacios, entre 2 y 50 caracteres."
        )]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's last name.
        /// Only letters and spaces are allowed (2-50 characters).
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,50}$",
            ErrorMessage = "El apellido solo puede contener letras y espacios, entre 2 y 50 caracteres."
        )]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's gender.
        /// Must be 'Masculino', 'Femenino' or 'Otro'.
        /// </summary>
        [Required(ErrorMessage = "El género es obligatorio.")]
        [RegularExpression(
            @"^(Masculino|Femenino|Otro)$",
            ErrorMessage = "El género debe ser 'Masculino', 'Femenino' u 'Otro'."
        )]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's birth date.
        /// Must be a valid date and the user must be over 18.
        /// </summary>
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(RegisterDTO), nameof(ValidateBirthDate))]
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the user's RUT (Chilean ID).
        /// Must be valid according to Chilean RUT rules.
        /// </summary>
        [Required(ErrorMessage = "El RUT es obligatorio.")]
        [CustomValidation(typeof(RegisterDTO), nameof(ValidateRUT))]
        public string Rut { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's email address.
        /// Must be a valid email format.
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's phone number.
        /// Must be in Chilean format: +569XXXXXXXX.
        /// </summary>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(
            @"^\+569\d{8}$",
            ErrorMessage = "El teléfono debe tener el formato chileno válido: +569XXXXXXXX."
        )]
        public string Phone { get; set; } = string.Empty;

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
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password confirmation.
        /// Must match the Password property.
        /// </summary>
        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare("Password", ErrorMessage = "La confirmación de contraseña no coincide.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Custom validation method for BirthDate.
        /// Ensures the date is not in the future and the user is at least 18 years old.
        /// </summary>
        public static ValidationResult? ValidateBirthDate(
            DateTime birthDate,
            ValidationContext context
        )
        {
            if (birthDate > DateTime.Today)
                return new ValidationResult("La fecha de nacimiento no puede ser futura.");

            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age))
                age--;

            if (age < 18)
                return new ValidationResult("Debes ser mayor de 18 años.");

            return ValidationResult.Success;
        }

        /// <summary>
        /// Custom validation method for RUT.
        /// Ensures the RUT is valid according to Chilean rules.
        /// </summary>
        public static ValidationResult? ValidateRUT(string rut, ValidationContext context)
        {
            rut = rut.Replace(".", "").Replace("-", "").ToUpper();
            if (rut.Length < 2)
                return new ValidationResult("RUT inválido.");

            string numberPart = rut[..^1];
            char dv = rut[^1];

            if (!int.TryParse(numberPart, out int rutNumber))
                return new ValidationResult("RUT inválido.");

            int sum = 0,
                multiplier = 2;
            while (rutNumber > 0)
            {
                sum += (rutNumber % 10) * multiplier;
                rutNumber /= 10;
                multiplier = multiplier == 7 ? 2 : multiplier + 1;
            }

            int remainder = 11 - (sum % 11);
            char expectedDV = remainder switch
            {
                11 => '0',
                10 => 'K',
                _ => remainder.ToString()[0],
            };

            return expectedDV == dv
                ? ValidationResult.Success
                : new ValidationResult("RUT inválido.");
        }
    }
}
