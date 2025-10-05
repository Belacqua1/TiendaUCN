using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Application.Services.Implements
{
    public class VerificationService : IVerificationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

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

        public async Task GenerateAndSendCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("Usuario no encontrado.");

            // Generar código aleatorio de 6 dígitos
            var code = new Random().Next(100000, 999999).ToString();

            // Guardar temporalmente el código (10 minutos)
            _cache.Set(email, code, TimeSpan.FromMinutes(10));

            // Enviar correo usando tu EmailService
            await _emailService.SendVerificationCodeEmailAsync(email, code);
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            if (!_cache.TryGetValue(email, out string? storedCode))
                return false;

            if (storedCode != code)
                return false;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            // Activar cuenta
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            // Limpiar código
            _cache.Remove(email);

            // Enviar correo de bienvenida
            await _emailService.SendWelcomeEmailAsync(email);

            return true;
        }
    }
}
