namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationCodeEmailAsync(string email, string code);
        Task SendWelcomeEmailAsync(string email);
    }
}
