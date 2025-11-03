using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.DTO.UserDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements
{
    /// <summary>
    /// Handles user-related operations such as registration, role assignment,
    /// and triggering email verification.
    /// </summary>
    public class UserService : IUserService
    {
        // Identity services to manage users and roles
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        // Service to generate and send verification codes
        private readonly IVerificationService _verificationService;

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/>.
        /// </summary>
        /// <param name="userManager">UserManager for managing user entities.</param>
        /// <param name="roleManager">RoleManager for managing user roles.</param>
        /// <param name="verificationService">Service for generating and sending verification codes.</param>
        public UserService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IVerificationService verificationService
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _verificationService = verificationService;
        }

        /// <summary>
        /// Registers a new user with the system, assigns the "Cliente" role,
        /// and triggers email verification.
        /// </summary>
        /// <param name="registerDto">Data transfer object containing user registration info.</param>
        /// <param name="clientIp">Optional client IP address for logging or auditing.</param>
        /// <returns>
        /// A <see cref="GenericResponse{T}"/> indicating success or failure,
        /// along with a message for the client.
        /// </returns>
        public async Task<GenericResponse<string>> RegisterAsync(
            RegisterDTO registerDto,
            string? clientIp = null
        )
        {
            // Check if the email already exists
            var existingEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingEmail != null)
                return new GenericResponse<string>(message: "The email already exists", data: null);

            // Check if the RUT is already registered
            var existingRut = await _userManager.Users.AnyAsync(u => u.Rut == registerDto.Rut);
            if (existingRut)
                return new GenericResponse<string>(message: "The RUT already exists", data: null);

            // Create a new user object
            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Rut = registerDto.Rut,
                Gender = registerDto.Gender,
                BirthDate = registerDto.BirthDate,
                RegisteredAt = DateTime.Now,
                PhoneNumber = registerDto.Phone,
                UpdatedAt = DateTime.Now,
                EmailConfirmed = false,
            };

            // Attempt to create the user with the specified password
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new GenericResponse<string>($"Error creating the user: {errors}", null);
            }

            // Ensure the "Cliente" role exists, otherwise create it
            if (!await _roleManager.RoleExistsAsync("Cliente"))
                await _roleManager.CreateAsync(new Role { Name = "Cliente" });

            // Assign the "Cliente" role to the user
            await _userManager.AddToRoleAsync(user, "Cliente");

            try
            {
                // Generate and send the verification code via email
                Console.WriteLine($"[DEBUG] Generating verification code for {user.Email}");
                await _verificationService.GenerateAndSendCodeAsync(
                    user.Email,
                    nameHtml: "VerificationCode"
                );
                Console.WriteLine($"[DEBUG] Code sent to {user.Email}");

                return new GenericResponse<string>(
                    "User registered successfully. Please verify your email.",
                    null
                );
            }
            catch (Exception ex)
            {
                // Log any error and return failure response
                Console.WriteLine($"[ERROR] Could not send email: {ex.Message}");
                return new GenericResponse<string>(
                    $"Could not send verification email: {ex.Message}",
                    null
                );
            }
        }

        public async Task<GenericResponse<string>> RecoverPasswordAsync(RecoverPasswordDTO dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser == null)
            {
                return new GenericResponse<string>(
                    message: "The email is not associated with any account",
                    data: null,
                    success: false
                );
            }

            await _verificationService.GenerateAndSendCodeAsync(dto.Email, nameHtml: "RecoverCode");

            return new GenericResponse<string>(
                message: "A verification code has been sent to your email.",
                data: null,
                success: true
            );
        }

        public async Task<GenericResponse<string>> ChangePasswordAsync(ResetPasswordDTO dto)
        {
            var isCodeValid = await _verificationService.VerifyCodeRecoverAsync(
                dto.Email,
                dto.Code
            );
            if (!isCodeValid)
            {
                return new GenericResponse<string>(
                    message: "Invalid or expired code",
                    data: null,
                    success: false
                );
            }
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new GenericResponse<string>(message: "User not found.", data: null);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new GenericResponse<string>(
                    message: $"Could not change password: {errors}",
                    data: null
                );
            }

            return new GenericResponse<string>(
                message: "Password changed successfully.",
                data: null
            );
        }

        public async Task<GenericResponse<UserProfileDto>> GetUserProfileAsync(int userId)
        {
            // Retrieve user using ASP.NET Identity
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                Log.Warning("Profile request failed: user {UserId} not found", userId);
                return new GenericResponse<UserProfileDto>(
                    message: "User not found.",
                    data: null,
                    success: false
                );
            }

            // Map User entity to DTO to avoid leaking sensitive data
            var profile = new UserProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                Rut = user.Rut,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
            };

            // Log profile access event
            Log.Information(
                "User {UserId} accessed their profile at {Timestamp}",
                user.Id,
                DateTime.UtcNow
            );

            return new GenericResponse<UserProfileDto>(
                message: "User found successfully.",
                data: profile,
                success: true
            );
        }

        public async Task<GenericResponse<string>> UpdateProfileAsync(
            int userId,
            UpdateProfileDTO dto
        )
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                Log.Warning("Profile update attempt failed: user {UserId} not found", userId);
                return new GenericResponse<string>("User not found.", null, false);
            }

            bool modified = false;

            // Validar y actualizar nombre
            if (!string.IsNullOrWhiteSpace(dto.FirstName))
            {
                user.FirstName = dto.FirstName.Trim();
                modified = true;
            }

            // Validar y actualizar apellido
            if (!string.IsNullOrWhiteSpace(dto.LastName))
            {
                user.LastName = dto.LastName.Trim();
                modified = true;
            }

            // Validar género
            if (!string.IsNullOrWhiteSpace(dto.Gender))
            {
                var validGenders = new[] { "Masculino", "Femenino", "Otro" };
                if (!validGenders.Contains(dto.Gender))
                    return new GenericResponse<string>("Invalid gender.", null, false);

                user.Gender = dto.Gender;
                modified = true;
            }

            // Validar fecha de nacimiento
            if (dto.BirthDate.HasValue)
            {
                user.BirthDate = dto.BirthDate.Value;
                modified = true;
            }

            // Validar y actualizar RUT
            if (!string.IsNullOrWhiteSpace(dto.Rut))
            {
                var existingRut = await _userManager.Users.AnyAsync(u =>
                    u.Rut == dto.Rut && u.Id != userId
                );
                if (existingRut)
                    return new GenericResponse<string>(
                        "The RUT is already registered.",
                        null,
                        false
                    );

                user.Rut = dto.Rut;
                modified = true;
            }

            // Validar y actualizar email
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingEmail = await _userManager.Users.AnyAsync(u =>
                    u.Email == dto.Email && u.Id != userId
                );
                if (existingEmail)
                    return new GenericResponse<string>(
                        "The email is already registered.",
                        null,
                        false
                    );

                if (dto.Email != user.Email)
                {
                    user.EmailConfirmed = false;
                    user.Email = dto.Email;
                    user.UserName = dto.Email;
                    await _verificationService.GenerateAndSendCodeAsync(
                        user.Email,
                        nameHtml: "VerificationCode"
                    );
                    Log.Information(
                        "Verification code sent for email change to {Email}",
                        dto.Email
                    );
                }

                modified = true;
            }

            // Validar y actualizar teléfono
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                user.PhoneNumber = dto.Phone;
                modified = true;
            }

            if (!modified)
                return new GenericResponse<string>(
                    "No valid fields were sent to update.",
                    null,
                    false
                );

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Log.Error("Error updating user profile {UserId}: {Errors}", userId, errors);
                return new GenericResponse<string>(
                    $"Error updating profile: {errors}",
                    null,
                    false
                );
            }

            Log.Information(
                "User {UserId} updated their profile on {Date}. Modified fields: {Fields}",
                userId,
                DateTime.Now,
                string.Join(
                    ", ",
                    typeof(UpdateProfileDTO)
                        .GetProperties()
                        .Where(p => p.GetValue(dto) != null)
                        .Select(p => p.Name)
                )
            );

            return new GenericResponse<string>("Profile updated successfully.", null, true);
        }

        public async Task<int> DeleteUnconfirmedUsersAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            var unconfirmedUsers = await _userManager
                .Users.Where(u => !u.EmailConfirmed && u.RegisteredAt < cutoffDate)
                .ToListAsync();

            int deletedCount = 0;

            foreach (var user in unconfirmedUsers)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    deletedCount++;
                    Log.Information("Unconfirmed user {UserId} deleted automatically.", user.Id);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Log.Error(
                        "Error deleting unconfirmed user {UserId}: {Errors}",
                        user.Id,
                        errors
                    );
                }
            }

            return deletedCount;
        }

        public async Task<GenericResponse<string>> ResendVerificationCodeAsync(string email)
        {
            // Check if user exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new GenericResponse<string>(
                    message: "User not found.",
                    data: null,
                    success: false
                );

            // Check if email is already verified
            if (user.EmailConfirmed)
                return new GenericResponse<string>(
                    message: "The email is already verified.",
                    data: null,
                    success: false
                );

            try
            {
                // Generate and send new verification code
                await _verificationService.GenerateAndSendCodeAsync(email, "VerificationCode");

                return new GenericResponse<string>(
                    message: "Verification code resent successfully.",
                    data: null,
                    success: true
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error resending verification code for {Email}", email);
                return new GenericResponse<string>(
                    message: $"Error resending code: {ex.Message}",
                    data: null,
                    success: false
                );
            }
        }
    }
}
