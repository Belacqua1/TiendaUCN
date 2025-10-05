namespace TiendaUCN.src.Application.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for email-related operations.
    /// Responsible for sending emails such as verification codes and welcome messages.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a verification code email to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <param name="code">The verification code to be included in the email.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendVerificationCodeEmailAsync(string email, string code);

        /// <summary>
        /// Sends a welcome email to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendWelcomeEmailAsync(string email);
    }
}
