namespace TiendaUCN.src.Application.DTO.AuthDTO
{
    /// <summary>
    /// Data transfer object used for verifying a user's email with a code.
    /// </summary>
    public class VerifyCodeDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user to verify.
        /// Initialized as an empty string to avoid null reference issues.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the verification code sent to the user's email.
        /// Initialized as an empty string to avoid null reference issues.
        /// </summary>
        public string Code { get; set; } = string.Empty;
    }
}
