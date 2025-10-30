namespace TiendaUCN.src.Application.DTO.UserDTO
{
    /// <summary>
    /// Data Transfer Object for returning user profile information.
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>
        /// User's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's gender (Masculino, Femenino, Otro).
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// User's date of birth.
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// User's RUT (unique Chilean identifier).
        /// </summary>
        public string Rut { get; set; } = string.Empty;

        /// <summary>
        /// User's registered email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's phone number in Chilean format.
        /// </summary>
        public string Phone { get; set; } = string.Empty;
    }
}
