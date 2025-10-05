using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationService"/> class.
        /// </summary>
        /// <param name="userManager">The UserManager used to manage user entities.</param>
        /// <param name="emailService">The email service used to send emails.</param>
        /// <param name="cache">The memory cache used to temporarily store verification codes.</param>
        public VerificationService(
            UserManager<User> userManager,
            IEmailService emailService,
            IMemoryCache cache
        )
        {
            _userManager = userManager;
            _emailService = emailService;
            _cache = cache;
        }

        /// <summary>
        /// Generates a random 6-digit verification code, stores it temporarily in cache,
        /// and sends it to the specified email address.
        /// </summary>
        /// <param name="email">The recipient's email address.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the user is not found.</exception>
        public async Task GenerateAndSendCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            // Generate a random 6-digit code
            var code = new Random().Next(100000, 999999).ToString();

            // Store the code in cache for 10 minutes
            _cache.Set(email, code, TimeSpan.FromMinutes(10));

            // Send verification code via email
            await _emailService.SendVerificationCodeEmailAsync(email, code);
        }

        /// <summary>
        /// Verifies the given code for the specified email, confirms the user's email,
        /// and sends a welcome email upon successful verification.
        /// </summary>
        /// <param name="email">The email address associated with the code.</param>
        /// <param name="code">The verification code provided by the user.</param>
        /// <returns>
        /// <c>true</c> if the code is valid and the email is confirmed; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            // Try to retrieve the stored code from cache
            if (!_cache.TryGetValue(email, out string? storedCode))
                return false;

            // Check if the provided code matches the stored code
            if (storedCode != code)
                return false;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            // Confirm the user's email
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            // Remove the code from cache
            _cache.Remove(email);

            // Send a welcome email
            await _emailService.SendWelcomeEmailAsync(email);

            return true;
        }
    }
}
