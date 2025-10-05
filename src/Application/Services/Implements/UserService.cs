using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaUCN.src.Application.DTO.AuthDTO;
using TiendaUCN.src.Application.DTO.BaseResponse;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        // Implementation of user registration logic
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRepository _userRepository;

        public UserService(UserManager<User> userManager, RoleManager<Role> roleManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepository = userRepository;

        }

        public async Task<int> DeleteUnconfirmedAsync()
        {
            return await _userRepository.DeleteUnconfirmedAsync();
        }

        public async Task<GenericResponse<string>> RegisterAsync(
            RegisterDTO registerDto,
            string? clientIp = null
        )
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new GenericResponse<string>("El usuario ya existe", null);
            }
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
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new GenericResponse<string>($"Error al crear el usuario: {errors}", null);
            }
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new Role { Name = "Customer" });
            }
            await _userManager.AddToRoleAsync(user, "Customer");
            return new GenericResponse<string>("Usuario registrado exitosamente", null);
        }
    }
}
