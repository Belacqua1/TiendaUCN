namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IVerificationService
    {
        Task GenerateAndSendCodeAsync(string email);

        Task<bool> VerifyCodeAsync(string email, string code);
    }
}
