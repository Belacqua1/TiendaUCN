using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        // Implementation of user registration logic
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IVerificationService _verificationService;

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

        public async Task<GenericResponse<string>> RegisterAsync(
            RegisterDTO registerDto,
            string? clientIp = null
        )
        {
            var existingEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingEmail != null)
                return new GenericResponse<string>(message: "El email ya existe", data: null);

            var existingRut = await _userManager.Users.AnyAsync(u => u.Rut == registerDto.Rut);
            if (existingRut)
                return new GenericResponse<string>(message: "El RUT ya existe", data: null);

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
                UpdatedAt = DateTime.Now,
                EmailConfirmed = false,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new GenericResponse<string>($"Error al crear el usuario: {errors}", null);
            }

            if (!await _roleManager.RoleExistsAsync("Cliente"))
                await _roleManager.CreateAsync(new Role { Name = "Cliente" });

            await _userManager.AddToRoleAsync(user, "Cliente");

            try
            {
                Console.WriteLine($"[DEBUG] Generando código de verificación para {user.Email}");
                await _verificationService.GenerateAndSendCodeAsync(user.Email);
                Console.WriteLine($"[DEBUG] Código enviado a {user.Email}");
                return new GenericResponse<string>(
                    "Usuario registrado exitosamente. Por favor, verifica tu email.",
                    null
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] No se pudo enviar correo: {ex.Message}");
                return new GenericResponse<string>(
                    $"No se pudo enviar el correo de verificación: {ex.Message}",
                    null
                );
            }
        }
    }
}
