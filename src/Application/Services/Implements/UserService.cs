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
                return new GenericResponse<string>(message: "El email ya existe", data: null);

            // Check if the RUT is already registered
            var existingRut = await _userManager.Users.AnyAsync(u => u.Rut == registerDto.Rut);
            if (existingRut)
                return new GenericResponse<string>(message: "El RUT ya existe", data: null);

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
                return new GenericResponse<string>($"Error al crear el usuario: {errors}", null);
            }

            // Ensure the "Cliente" role exists, otherwise create it
            if (!await _roleManager.RoleExistsAsync("Cliente"))
                await _roleManager.CreateAsync(new Role { Name = "Cliente" });

            // Assign the "Cliente" role to the user
            await _userManager.AddToRoleAsync(user, "Cliente");

            try
            {
                // Generate and send the verification code via email
                Console.WriteLine($"[DEBUG] Generando código de verificación para {user.Email}");
                await _verificationService.GenerateAndSendCodeAsync(
                    user.Email,
                    nameHtml: "VerificationCode"
                );
                Console.WriteLine($"[DEBUG] Código enviado a {user.Email}");

                return new GenericResponse<string>(
                    "Usuario registrado exitosamente. Por favor, verifica tu email.",
                    null
                );
            }
            catch (Exception ex)
            {
                // Log any error and return failure response
                Console.WriteLine($"[ERROR] No se pudo enviar correo: {ex.Message}");
                return new GenericResponse<string>(
                    $"No se pudo enviar el correo de verificación: {ex.Message}",
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
                    message: "El email no está asociado a ninguna cuenta",
                    data: null,
                    success: false
                );
            }

            await _verificationService.GenerateAndSendCodeAsync(dto.Email, nameHtml: "RecoverCode");

            return new GenericResponse<string>(
                message: "Se ha enviado un código de verificación a su correo electrónico.",
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
                    message: "Código inválido o expirado",
                    data: null,
                    success: false
                );
            }
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new GenericResponse<string>(message: "Usuario no encontrado.", data: null);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new GenericResponse<string>(
                    message: $"No se pudo cambiar la contraseña: {errors}",
                    data: null
                );
            }

            return new GenericResponse<string>(
                message: "Contraseña cambiada correctamente.",
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
                    message: "Usuario no encontrado.",
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
            };

            // Log profile access event
            Log.Information(
                "User {UserId} accessed their profile at {Timestamp}",
                user.Id,
                DateTime.UtcNow
            );

            return new GenericResponse<UserProfileDto>(
                message: "Usuario encontrado exitosamente.",
                data: profile,
                success: true
            );
        }
    }
}
