namespace TiendaUCN.src.Application.Services.Interfaces
{
    public interface IImageService
    {
        Task<bool> UploadImageAsync(IFormFile file, int productId);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
