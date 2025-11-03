using Microsoft.AspNetCore.Identity;
using Serilog;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements
{
    /// <summary>
    /// Handles user email verification logic, including generating verification codes,
    /// validating them, confirming user email, and sending welcome emails.
    /// </summary>
    public class VerificationService : IVerificationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationService"/> class.
        /// </summary>
        /// <param name="userManager">The UserManager used to manage user entities.</param>
        /// <param name="emailService">The email service used to send emails.</param>
        public VerificationService(UserManager<User> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task GenerateAndSendCodeAsync(
            string email,
            string nameHtml,
            bool isPendingEmail = false
        )
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new Exception("User not found.");

            // Generate 6-digit code
            var code = new Random().Next(100000, 999999).ToString();
            Log.Information(
                $"[VERIFICATION DEBUG] (this is for test)Code generated for {email}: {code}"
            );

            user.VerificationCode = code;
            user.VerificationCodeExpires = DateTime.UtcNow.AddMinutes(10);

            // If it's email change verification
            if (isPendingEmail && user.PendingEmail != null)
            {
                await _emailService.SendVerificationCodeEmailAsync(
                    user.PendingEmail,
                    code,
                    nameHtml
                );
            }
            else
            {
                await _emailService.SendVerificationCodeEmailAsync(email, code, nameHtml);
            }

            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            // Validations
            if (user.VerificationCode == null || user.VerificationCodeExpires == null)
                return false;

            if (DateTime.UtcNow > user.VerificationCodeExpires)
                return false;

            if (user.VerificationCode != code)
                return false;

            // Confirm email and clear code
            user.EmailConfirmed = true;
            user.VerificationCode = null;
            user.VerificationCodeExpires = null;

            await _userManager.UpdateAsync(user);
            await _emailService.SendWelcomeEmailAsync(email);

            return true;
        }

        public async Task<bool> VerifyPendingEmailAsync(string currentEmail, string code)
        {
            var user = await _userManager.FindByEmailAsync(currentEmail);
            if (user == null)
                return false;

            if (string.IsNullOrEmpty(user.PendingEmail))
                return false;

            if (user.VerificationCode == null || user.VerificationCodeExpires == null)
                return false;

            if (DateTime.UtcNow > user.VerificationCodeExpires)
                return false;

            if (user.VerificationCode != code)
                return false;

            // Update the real email with the pending one
            user.Email = user.PendingEmail;
            user.UserName = user.PendingEmail; // Identity usually uses email as username
            user.NormalizedEmail = user.PendingEmail.ToUpper();
            user.NormalizedUserName = user.PendingEmail.ToUpper();
            user.EmailConfirmed = true;

            // Clear temporary data
            user.PendingEmail = null;
            user.VerificationCode = null;
            user.VerificationCodeExpires = null;

            await _userManager.UpdateAsync(user);

            return true;
        }

        public async Task<bool> VerifyCodeRecoverAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            if (user.VerificationCode == null || user.VerificationCodeExpires == null)
                return false;

            if (DateTime.UtcNow > user.VerificationCodeExpires)
                return false;

            if (user.VerificationCode != code)
                return false;

            // Clear the code after verification
            user.VerificationCode = null;
            user.VerificationCodeExpires = null;
            await _userManager.UpdateAsync(user);

            return true;
        }
    }
}
