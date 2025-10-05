namespace TiendaUCN.src.Application.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for the verification service.
    /// Responsible for generating, sending, and validating verification codes for user emails.
    /// </summary>
    public interface IVerificationService
    {
        /// <summary>
        /// Generates a verification code and sends it to the specified email address.
        /// </summary>
        /// <param name="email">The email address to send the verification code to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task GenerateAndSendCodeAsync(string email);

        /// <summary>
        /// Validates a given verification code for a specific email address.
        /// </summary>
        /// <param name="email">The email address associated with the verification code.</param>
        /// <param name="code">The verification code provided by the user.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing <c>true</c> if the code is valid; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> VerifyCodeAsync(string email, string code);
    }
}
